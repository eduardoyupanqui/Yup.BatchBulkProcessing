using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yup.BulkProcess;

public class SeguimientoProcesoBloqueService : ISeguimientoProcesoBloqueService
{
    public Func<SeguimientoProcesoArchivoEventArgs, Task> ProcessStartedAsync { private get; set; }
    public Func<SeguimientoProcesoArchivoEventArgs, Task> StatusUpdateAsync { private get; set; }
    public Func<SeguimientoProcesoArchivoEventArgs, Task> ProcessCompletedAsync { private get; set; }
    public SeguimientoProcesoBloqueService()
    {

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

    private Task LimpiarLlavesDeProcesoArchivo(Guid idArchivoCarga)
    {
        var keyResumenProcesoArchivo = $"proc:{idArchivoCarga}:summary";
        var keyDirectorioBloquesProcesoArchivo = $"proc:{idArchivoCarga}:blocks";
        return Task.CompletedTask;
    }
    private async Task InicializarSummaryProgresoArchivo(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyResumenProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga.ToString()}:summary";

        await(ProcessStartedAsync?.Invoke(new SeguimientoProcesoArchivoEventArgs()
        {
            IdArchivoCarga = eventArgs.IdArchivoCarga,
            IdEntidad = eventArgs.IdEntidad,
            CodigoEntidad = eventArgs.CodigoEntidad,
        }) ?? Task.CompletedTask);
    }

    private Task MatricularBloqueActualEnDirectorioDeBloquesDelArchivo(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyDirectorioBloquesProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga}:blocks";
        var keyBloqueSummary = $"proc:{eventArgs.IdArchivoCarga}:{eventArgs.IdBloque}";
        return Task.CompletedTask;
    }

    private Task ActualizarContadoresDeProcesoBloque(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyBloqueSummary = $"proc:{eventArgs.IdArchivoCarga}:{eventArgs.IdBloque}";
        return Task.CompletedTask;
    }

    private async Task SumarizarContadoresDeProcesoArchivo(ProcesoArchivoCargaEventArgs eventArgs)
    {
        var keyResumenProcesoArchivo = $"proc:{eventArgs.IdArchivoCarga}:summary";
        ContadoresProceso contadoresArchivo = new ContadoresProceso();

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
        ContadoresProceso summary = ObtenerContadoresProcesoDeKey(keyResumenProcesoArchivo);
        await(ProcessCompletedAsync?.Invoke(new SeguimientoProcesoArchivoEventArgs()
        {
            IdArchivoCarga = eventArgs.IdArchivoCarga,
            IdEntidad = eventArgs.IdEntidad,
            CodigoEntidad = eventArgs.CodigoEntidad,
            ContadoresProceso = summary
        }) ?? Task.CompletedTask);
    }

    private ContadoresProceso ObtenerContadoresProcesoDeKey(string summaryKey) 
    {
        ContadoresProceso result = new ContadoresProceso();
        if (string.IsNullOrWhiteSpace(summaryKey)) return result;


        return result;
    }
}
