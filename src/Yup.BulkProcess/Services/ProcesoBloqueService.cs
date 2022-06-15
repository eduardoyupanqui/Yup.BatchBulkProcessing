using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Domain.SeedworkMongoDB;
using Yup.Validation;

namespace Yup.BulkProcess;

public sealed class ProcesoBloqueService<TBloque, TFila, TFilaModel> : MantenimientoProcesoService<TBloque, TFila>,
                                                      IProcesoBloqueService
                                                      where TBloque : BloqueCarga<TFila>
                                                      where TFila : FilaArchivoCarga
                                                      where TFilaModel : IValidable
{
    private IValidator<TFilaModel> _modelValidator;
    private IFilaArchivoCargaConverter<TFila, TFilaModel> _converter;
    private string _currentServiceId;

    public Func<ProcesoArchivoCargaEventArgs, Task> InicioProcesoAsync { private get; set; }
    public Func<ProcesoArchivoCargaEventArgs, Task> ProgresoProcesoAsync { private get; set; }
    public Func<ProcesoArchivoCargaEventArgs, Task> FinProcesoAsync { private get; set; }


    public ProcesoBloqueService(IBloqueCargaGenericRepository bloqueGenericRepository,
                                     IFilaArchivoCargaConverter<TFila, TFilaModel> converter,
                                     IValidator<TFilaModel> modelValidator,
                                     ILogger logger) : base(bloqueGenericRepository, logger)
    {
        _currentServiceId = Guid.NewGuid().ToString();
        _modelValidator = modelValidator ?? throw new ArgumentNullException(nameof(modelValidator));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    public override void SetAuditoriaInfo(string usuarioAutor, string ipOrigen, string hostNameOrigen)
    {
        #region Validacion/Asimilacion de parámetros de entrada
        Guid tmpGuidUsuarioAutor;
        if (Guid.TryParse(usuarioAutor, out tmpGuidUsuarioAutor) == false) throw new ArgumentNullException(nameof(usuarioAutor));
        if (string.IsNullOrWhiteSpace(ipOrigen)) throw new ArgumentNullException(nameof(ipOrigen));
        if (string.IsNullOrWhiteSpace(hostNameOrigen)) throw new ArgumentNullException(nameof(hostNameOrigen));

        _usuarioAutor = usuarioAutor;
        _ipOrigen = ipOrigen;
        _hostNameOrigen = hostNameOrigen;
        #endregion
    }
    public async Task ProcesarBloquesDeArchivoAsync(Guid idArchivoCarga)
    {
        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);

        if (objArchivoCarga.Estado == EstadoCarga.PENDIENTE)
        {
            #region 1) Marcando el archivo como "en evaluación"
            var updArchivoEnEvaluacion = Builders<ArchivoCarga>.Update
                                                 .Set(b => b.Estado, EstadoCarga.EN_EVALUACION)
                                                 .Set(b => b.FechaEvaluacionInicio, DateTime.Now)
                                                 .Set(b => b.UsuarioModificacion, _usuarioAutor)
                                                 .Set(b => b.FechaModificacion, DateTime.Now);

            _bloqueGenericRepository.UpdateOne(objArchivoCarga, updArchivoEnEvaluacion);
            #endregion

            #region 2) Generando evento "Inicio de proceso"
            await InicioProcesoAsync(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Inicio)
            {
                IdArchivoCarga = idArchivoCarga,
                IdEntidad = objArchivoCarga.IdEntidad,
                CodigoEntidad = objArchivoCarga.CodigoEntidad,
                ContadoresProceso = new ContadoresProceso()
                {
                    TotalElementos = objArchivoCarga.CantidadTotalElementos
                }
            });
            #endregion
        }

        bool readModel = true;
        bool readFilaModel = false;
        while (await ProcesarBloque(objArchivoCarga, readModel, readFilaModel))
        {
        }
        await FinalizarProcesoDeBloquesDeArchivo(idArchivoCarga);
    }

    private async Task<bool> ProcesarBloque(ArchivoCarga objArchivoCarga, bool readModel, bool readFilaModel)
    {
        ContadoresProceso contadoresBloque = null;
        TBloque objBloqueDeBD = null; //para pruebas de verificación;

        var objBloqueEnProceso = ObtenerSiguienteBloqueAProcesar(objArchivoCarga.Id);
        if (objBloqueEnProceso == null) //Si ya no existen bloques que procesar
        {
            return false;
        }

        var currentBloqueId = objBloqueEnProceso.Id;
        #region Marcando el bloque como "En evaluación"
        var updBloqueEnProceso = Builders<TBloque>.Update
            .Set(b => b.Estado, EstadoCarga.EN_EVALUACION)
            .Set(b => b.FechaEvaluacionInicio, DateTime.Now)
            .Set(b => b.UsuarioModificacion, _usuarioAutor)
            .Set(b => b.IdProcesador, _ipOrigen)
            .Set(b => b.HostnameProcesador, _hostNameOrigen);

        _bloqueGenericRepository.UpdateOne<TBloque>(objBloqueEnProceso, updBloqueEnProceso);
        #endregion

        objBloqueDeBD = _bloqueGenericRepository.GetById<TBloque>(currentBloqueId);

        contadoresBloque = new ContadoresProceso();
        contadoresBloque.TotalElementos = objBloqueDeBD.CantidadTotalElementos;

        TFila filaEnProceso = null;

        for (int i = 0; i < objBloqueDeBD.Filas.Count; i++)
        {
            filaEnProceso = objBloqueDeBD.Filas[i];
            var validationResult = filaEnProceso.ValidarAtributos();

            if (validationResult.EsValido)
            {
                var currentModel = _converter.CovertToModel(filaEnProceso);
                validationResult = _modelValidator.Validar(currentModel);
                if (readModel)
                {
                    objBloqueDeBD.Filas[i] = _converter.ReadModel(currentModel);
                    filaEnProceso = objBloqueDeBD.Filas[i];
                }
                if (readFilaModel)
                {
                    objBloqueDeBD.Filas[i] = _converter.ReadFilaModel(currentModel, filaEnProceso);
                    filaEnProceso = objBloqueDeBD.Filas[i];
                }
            }

            filaEnProceso.NumeroFila = objBloqueDeBD.FilaInicial + i;

            #region Asimilacion de resultados de validacion
            filaEnProceso.Evaluado = true;
            filaEnProceso.EsValido = validationResult.EsValido;
            filaEnProceso.Observaciones.AddRange(validationResult.Observaciones);
            #endregion

            #region Contabilizar procesados, validados y observados
            if (filaEnProceso.Evaluado) { contadoresBloque.Evaluados++; }
            if (filaEnProceso.Evaluado && filaEnProceso.EsValido) { contadoresBloque.EvaluadosValidos++; }
            if (filaEnProceso.Evaluado && !filaEnProceso.EsValido) { contadoresBloque.EvaluadosObservados++; }
            #endregion

            await ProgresoProcesoAsync(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Progreso)
            {
                IdArchivoCarga = objArchivoCarga.Id,
                IdBloque = currentBloqueId,
                IdEntidad = objArchivoCarga.IdEntidad,
                CodigoEntidad = objArchivoCarga.CodigoEntidad,
                ContadoresProceso = contadoresBloque
            });
        }

        #region Marcando el bloque como "Evaluado"
        EstadoCarga estadoBloque = contadoresBloque.EvaluadosObservados == 0 ? EstadoCarga.EVALUADO_CORRECTO : EstadoCarga.EVALUADO_CON_OBSERVACIONES;

        var updBloqueProcesado = Builders<TBloque>.Update
           .Set(b => b.Estado, estadoBloque)
           .Set(b => b.FechaEvaluacionFin, DateTime.Now)
           .Set(b => b.IdProcesador, _currentServiceId)
           .Set(b => b.HostnameProcesador, _hostNameOrigen)

           .Set(b => b.CantidadTotalElementos, contadoresBloque.TotalElementos)
           .Set(b => b.CantidadEvaluados, contadoresBloque.Evaluados)
           .Set(b => b.CantidadEvaluadosValidos, contadoresBloque.EvaluadosValidos)
           .Set(b => b.CantidadEvaluadosObservados, contadoresBloque.EvaluadosObservados)
           .Set(b => b.Filas, objBloqueDeBD.Filas)

           .Set(b => b.FechaModificacion, DateTime.Now)
           .Set(b => b.UsuarioModificacion, _usuarioAutor)
           .Set(b => b.IpModificacion, _ipOrigen)
           ;

        _bloqueGenericRepository.UpdateOne<TBloque>(objBloqueEnProceso, updBloqueProcesado);
        #endregion
        return true;
    }

    private TBloque ObtenerSiguienteBloqueAProcesar(Guid idArchivoCarga)
    {
        var objSiguienteBloqueAProcesar = _bloqueGenericRepository.GetOne<TBloque>(x =>
                                                    x.IdCarga == idArchivoCarga &&
                                                    x.Estado == EstadoCarga.PENDIENTE &&
                                                    x.EsActivo == true &&
                                                    x.EsEliminado == false);

        _logger.LogDebug($"Verificando si existen bloques por procesar...{objSiguienteBloqueAProcesar != null}");

        return objSiguienteBloqueAProcesar;
    }

    private async Task FinalizarProcesoDeBloquesDeArchivo(Guid idArchivoCarga)
    {
        _logger.LogDebug($"Finalizando el proceso del archivo de carga id = {idArchivoCarga.ToString()}");
        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);

        #region 1) Sumarizando contadores de archivo y marcando el registro como "Procesado"

        ContadoresProceso contadorArchivo = null;
        SumarizarContadoresDeBloquesDeArchivo(idArchivoCarga, out contadorArchivo);

        var updFinProcesamiento = Builders<ArchivoCarga>.Update
            .Set(b => b.FechaEvaluacionFin, DateTime.Now)
            .Set(b => b.FechaModificacion, DateTime.Now)
            .Set(b => b.IpModificacion, _ipOrigen)
            .Set(b => b.UsuarioModificacion, _usuarioAutor);

        _bloqueGenericRepository.UpdateOne(objArchivoCarga, updFinProcesamiento);

        #endregion
        #region 2) Generando evento Finalizacion de proceso
        await FinProcesoAsync(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Fin)
        {
            IdArchivoCarga = idArchivoCarga,
            IdEntidad = objArchivoCarga.IdEntidad,
            CodigoEntidad = objArchivoCarga.CodigoEntidad,
            ContadoresProceso = contadorArchivo ?? new ContadoresProceso()
        });
        #endregion
    }
}
