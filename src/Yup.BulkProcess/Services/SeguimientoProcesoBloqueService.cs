using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Yup.BulkProcess;

public class SeguimientoProcesoBloqueService : ISeguimientoProcesoBloqueService
{
    const string _campoTotalElementos = "Total";
    const string _campoEvaluados = "Evaluados";
    const string _campoEvaluadosValidos = "EvaluadosValidos";
    const string _campoEvaluadosObservados = "EvaluadosObservados";
    const string _campoRegistradosValidos = "RegistradosValidos";
    const string _campoRegistradosFallidos = "RegistradosFallidos";

    private readonly IRedisDatabase _redisDatabase;
    public Func<SeguimientoProcesoArchivoEventArgs, Task> ProcessStartedAsync { private get; set; }
    public Func<SeguimientoProcesoArchivoEventArgs, Task> StatusUpdateAsync { private get; set; }
    public Func<SeguimientoProcesoArchivoEventArgs, Task> ProcessCompletedAsync { private get; set; }
    public SeguimientoProcesoBloqueService(IRedisDatabase redisDatabase)
    {
        _redisDatabase = redisDatabase;
    }
    public async Task ProcesarMensajeProgresoAsync(ProcesoArchivoCargaEventArgs eventArgs)
    {
        if (eventArgs == null) return;

        switch (eventArgs.TipoEvento)
        {
            case TipoEventoProcesoArchivo.Inicio:
                await LimpiarLlavesDeProcesoArchivo(eventArgs.IdArchivoCarga);
                await InicializarSummaryProgresoArchivo(eventArgs);
                break;
            case TipoEventoProcesoArchivo.Progreso:
                await MatricularBloqueActualEnDirectorioDeBloquesDelArchivo(eventArgs);
                await ActualizarContadoresDeProcesoBloque(eventArgs);
                await SumarizarContadoresDeProcesoArchivo(eventArgs);
                break;
            case TipoEventoProcesoArchivo.Fin:
                await SumarizarContadoresDeProcesoArchivo(eventArgs);
                await NotificarFinalizacionDeProceso(eventArgs);
                await LimpiarLlavesDeProcesoArchivo(eventArgs.IdArchivoCarga);
                break;
            default:
                break;
        }
    }

    private async Task LimpiarLlavesDeProcesoArchivo(Guid idArchivoCarga)
    {
        var keyResumenProcesoArchivo = $"proc:{idArchivoCarga}:summary";
        var keyDirectorioBloquesProcesoArchivo = $"proc:{idArchivoCarga}:blocks";

        var listKeys = new List<string>() { keyResumenProcesoArchivo, keyDirectorioBloquesProcesoArchivo };
        var response = await _redisDatabase.RemoveAllAsync(listKeys);
    }
    private async Task InicializarSummaryProgresoArchivo(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyResumenProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga.ToString()}:summary";
        var response = await _redisDatabase.Database.HashSetAsync(keyResumenProcesoArchivo,
                                                 _campoTotalElementos,
                                                 eventArgs.ContadoresProceso.TotalElementos,
                                                 When.NotExists);

        await (ProcessStartedAsync?.Invoke(new SeguimientoProcesoArchivoEventArgs()
        {
            IdArchivoCarga = eventArgs.IdArchivoCarga,
            IdEntidad = eventArgs.IdEntidad,
            CodigoEntidad = eventArgs.CodigoEntidad,
        }) ?? Task.CompletedTask);
    }

    private async Task MatricularBloqueActualEnDirectorioDeBloquesDelArchivo(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyDirectorioBloquesProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga}:blocks";
        var keyBloqueSummary = $"proc:{eventArgs.IdArchivoCarga}:{eventArgs.IdBloque}";
        var response = await _redisDatabase.SetAddAsync(keyDirectorioBloquesProcesoArchivo, keyBloqueSummary, CommandFlags.FireAndForget);
    }

    private async Task ActualizarContadoresDeProcesoBloque(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyBloqueSummary = $"proc:{eventArgs.IdArchivoCarga}:{eventArgs.IdBloque}";
        var listTasks = new List<Task>();

        //1) Total de elementos
        listTasks.Add(_redisDatabase.Database.HashSetAsync(keyBloqueSummary,
                                         _campoTotalElementos,
                                         eventArgs.ContadoresProceso.TotalElementos,
                                         When.NotExists,
                                         CommandFlags.FireAndForget));
        //2) Elementos evaluados
        listTasks.Add(_redisDatabase.Database.HashSetAsync(keyBloqueSummary,
                                            _campoEvaluados,
                                            eventArgs.ContadoresProceso.Evaluados,
                                            When.Always,
                                            CommandFlags.FireAndForget));

        //3) Elementos Evaluados Observados
        listTasks.Add(_redisDatabase.Database.HashSetAsync(keyBloqueSummary,
                                            _campoEvaluadosObservados,
                                            eventArgs.ContadoresProceso.EvaluadosObservados,
                                            When.Always,
                                            CommandFlags.FireAndForget));

        //4) Elementos Evaluados Válidos
        listTasks.Add(_redisDatabase.Database.HashSetAsync(keyBloqueSummary,
                                            _campoEvaluadosValidos,
                                            eventArgs.ContadoresProceso.EvaluadosValidos,
                                            When.Always,
                                            CommandFlags.FireAndForget));

        //4) Elementos Registrados Válidos
        listTasks.Add(_redisDatabase.Database.HashSetAsync(keyBloqueSummary,
                                            _campoRegistradosValidos,
                                            eventArgs.ContadoresProceso.RegistradosValidos,
                                            When.Always,
                                            CommandFlags.FireAndForget));

        //5) Elementos Registrados Fallidos
        listTasks.Add(_redisDatabase.Database.HashSetAsync(keyBloqueSummary,
                                            _campoRegistradosFallidos,
                                            eventArgs.ContadoresProceso.RegistradosFallidos,
                                            When.Always,
                                            CommandFlags.FireAndForget));

        //Esperando ejecución de batch de instrucciones
        await Task.WhenAll(listTasks.ToArray());
    }

    private async Task SumarizarContadoresDeProcesoArchivo(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyResumenProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga.ToString()}:summary";
        var keyDirectorioBloquesProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga.ToString()}:blocks";

        var listKeysBloques = _redisDatabase.Database.SetMembers(keyDirectorioBloquesProcesoArchivo).Select(x => x.ToString()).ToList();
        var contadoresArchivo = new ContadoresProceso();
        //Obteniendo total de registros del archivo

        var rdvTotalRegistrosArchivo = _redisDatabase.Database.HashGet(keyResumenProcesoArchivo, _campoTotalElementos);
        contadoresArchivo.TotalElementos = (rdvTotalRegistrosArchivo.HasValue) ? (int)rdvTotalRegistrosArchivo : 0;

        ContadoresProceso contadoresBloqueActual;
        foreach (var keyBloque in listKeysBloques)
        {
            contadoresBloqueActual = await ObtenerContadoresProcesoDeKey(keyBloque);
            contadoresArchivo.Evaluados += contadoresBloqueActual.Evaluados;
            contadoresArchivo.EvaluadosObservados += contadoresBloqueActual.EvaluadosObservados;
            contadoresArchivo.EvaluadosValidos += contadoresBloqueActual.EvaluadosValidos;
            contadoresArchivo.RegistradosValidos += contadoresBloqueActual.RegistradosValidos;
            contadoresArchivo.RegistradosFallidos += contadoresBloqueActual.RegistradosFallidos;
        }
        var listTasks = new List<Task>();
        //Actualizando los contadores del summary con los resultados de la suma de bloques
        listTasks.Add(_redisDatabase.HashSetAsync(keyResumenProcesoArchivo, _campoEvaluados, contadoresArchivo.Evaluados));
        listTasks.Add(_redisDatabase.HashSetAsync(keyResumenProcesoArchivo, _campoEvaluadosObservados, contadoresArchivo.EvaluadosObservados));
        listTasks.Add(_redisDatabase.HashSetAsync(keyResumenProcesoArchivo, _campoEvaluadosValidos, contadoresArchivo.EvaluadosValidos));
        listTasks.Add(_redisDatabase.HashSetAsync(keyResumenProcesoArchivo, _campoRegistradosValidos, contadoresArchivo.RegistradosValidos));
        listTasks.Add(_redisDatabase.HashSetAsync(keyResumenProcesoArchivo, _campoRegistradosFallidos, contadoresArchivo.RegistradosFallidos));
        //Esperando ejecución de batch de instrucciones
        await Task.WhenAll(listTasks.ToArray());

        await (StatusUpdateAsync?.Invoke(new SeguimientoProcesoArchivoEventArgs()
        {
            IdArchivoCarga = eventArgs.IdArchivoCarga,
            IdBloque = eventArgs.IdBloque,
            IdEntidad = eventArgs.IdEntidad,
            CodigoEntidad = eventArgs.CodigoEntidad,
            ContadoresProceso = contadoresArchivo
        }) ?? Task.CompletedTask);
    }

    private async Task NotificarFinalizacionDeProceso(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyResumenProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga}:summary";
        var summary = await ObtenerContadoresProcesoDeKey(keyResumenProcesoArchivo);
        await(ProcessCompletedAsync?.Invoke(new SeguimientoProcesoArchivoEventArgs()
        {
            IdArchivoCarga = eventArgs.IdArchivoCarga,
            IdEntidad = eventArgs.IdEntidad,
            CodigoEntidad = eventArgs.CodigoEntidad,
            ContadoresProceso = summary
        }) ?? Task.CompletedTask);
    }

    private async Task<ContadoresProceso> ObtenerContadoresProcesoDeKey(string summaryKey) 
    {
        var result = new ContadoresProceso();
        if (string.IsNullOrWhiteSpace(summaryKey)) return result;

        var objSummaryBloque = await _redisDatabase.Database.HashGetAllAsync(summaryKey);

        var dictionary = objSummaryBloque.ToDictionary(
            k => k.Name.ToString(), 
            v => (int)v.Value);

        result.TotalElementos = dictionary.ContainsKey(_campoTotalElementos) ? dictionary[_campoTotalElementos] : 0;
        result.Evaluados = dictionary.ContainsKey(_campoEvaluados) ? dictionary[_campoEvaluados] : 0;
        result.EvaluadosObservados = dictionary.ContainsKey(_campoEvaluadosObservados) ? dictionary[_campoEvaluadosObservados] : 0;
        result.EvaluadosValidos = dictionary.ContainsKey(_campoEvaluadosValidos) ? dictionary[_campoEvaluadosValidos] : 0;
        result.RegistradosValidos = dictionary.ContainsKey(_campoRegistradosValidos) ? dictionary[_campoRegistradosValidos] : 0;
        result.RegistradosFallidos = dictionary.ContainsKey(_campoRegistradosFallidos) ? dictionary[_campoRegistradosFallidos] : 0;

        return result;
    }
}
