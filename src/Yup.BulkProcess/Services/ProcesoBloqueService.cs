using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                                                      IProcesoBloqueService<TBloque, TFila, TFilaModel>
                                                      where TBloque : BloqueCarga<TFila>
                                                      where TFila : FilaArchivoCarga
                                                      where TFilaModel : IValidable
{
    private static List<string> lstIdsPropiedadesListas = new List<string> { "System.Collections.Generic.IEnumerable" };
    private const int frecuenciaActualizacionAvanceProcesoEstandar = 100; //"Notificar Cada"
    private const int frecuenciaActualizacionAvanceProcesoMinimo = 10; //"Notificar Cada"

    private IValidator<TFilaModel> _modelValidator;
    private IFilaArchivoCargaConverter<TFila, TFilaModel> _converter;
    private string _currentServiceId;

    public Func<ProcesoArchivoCargaEventArgs, Task> InicioProcesoAsync { private get; set; }
    public Func<ProcesoArchivoCargaEventArgs, Task> ProgresoProcesoAsync { private get; set; }
    public Func<ProcesoArchivoCargaEventArgs, Task> FinProcesoAsync { private get; set; }

    private readonly ISeguimientoProcesoBloqueService _seguimientoBloqueService;

    public ProcesoBloqueService(IBloqueCargaGenericRepository bloqueGenericRepository,
                                     IFilaArchivoCargaConverter<TFila, TFilaModel> converter,
                                     ISeguimientoProcesoBloqueService seguimientoBloqueService,
                                     ILogger<ProcesoBloqueService<TBloque, TFila, TFilaModel>> logger) : base(bloqueGenericRepository, logger)
    {
        _currentServiceId = Guid.NewGuid().ToString();
        _converter = converter!;
        _seguimientoBloqueService = seguimientoBloqueService!;
        EnlazarEventosDeServicioDeSeguimiento();
    }

    public override void SetAuditoriaInfo(string usuarioAutor, string ipOrigen, string hostNameOrigen)
    {
        #region Validacion/Asimilacion de parámetros de entrada
        if (Guid.TryParse(usuarioAutor, out _) == false) throw new ArgumentNullException(nameof(usuarioAutor));
        if (string.IsNullOrWhiteSpace(ipOrigen)) throw new ArgumentNullException(nameof(ipOrigen));
        if (string.IsNullOrWhiteSpace(hostNameOrigen)) throw new ArgumentNullException(nameof(hostNameOrigen));

        _usuarioAutor = usuarioAutor;
        _ipOrigen = ipOrigen;
        _hostNameOrigen = hostNameOrigen;
        #endregion
    }

    public void SetValidator(IValidator<TFilaModel> modelValidator)
    {
        _modelValidator = modelValidator ?? throw new ArgumentNullException(nameof(modelValidator));
    }

    private void EnlazarEventosDeServicioDeSeguimiento()
    {
        _seguimientoBloqueService.ProcessStartedAsync = OnSeguimientoProcesoArchivoIniciado;
        _seguimientoBloqueService.StatusUpdateAsync = OnSeguimientoProcesoArchivoStatusUpdate;
        _seguimientoBloqueService.ProcessCompletedAsync = OnSeguimientoProcesoArchivoCompletado;
    }
    Task OnSeguimientoProcesoArchivoIniciado(SeguimientoProcesoArchivoEventArgs args)
    {
        return InicioProcesoAsync?.Invoke(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Inicio)
        {
            IdArchivoCarga = args.IdArchivoCarga,
            IdEntidad = args.IdEntidad,
            CodigoEntidad = args.CodigoEntidad,
            ContadoresProceso = args.ContadoresProceso
        }) ?? Task.CompletedTask;
    }
    Task OnSeguimientoProcesoArchivoStatusUpdate(SeguimientoProcesoArchivoEventArgs args) 
    {
        return ProgresoProcesoAsync?.Invoke(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Progreso)
        {
            IdArchivoCarga = args.IdArchivoCarga,
            IdBloque = args.IdBloque,
            IdEntidad = args.IdEntidad,
            CodigoEntidad = args.CodigoEntidad,
            ContadoresProceso = args.ContadoresProceso
        }) ?? Task.CompletedTask;
    }
    Task OnSeguimientoProcesoArchivoCompletado(SeguimientoProcesoArchivoEventArgs args)
    {
        return FinProcesoAsync?.Invoke(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Fin)
        {
            IdArchivoCarga = args.IdArchivoCarga,
            IdEntidad = args.IdEntidad,
            CodigoEntidad = args.CodigoEntidad,
            ContadoresProceso = args.ContadoresProceso ?? new ContadoresProceso()
        }) ?? Task.CompletedTask;
    }
    public async Task ProcesarBloquesDeArchivoAsync(Guid idArchivoCarga)
    {
        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);
        if (objArchivoCarga.Estado == EstadoCarga.PENDIENTE)
        {
            await IniciarProcesoDeBloquesDeArchivo(objArchivoCarga);
        }

        bool readModel = true;
        bool readFilaModel = false;
        while (await ProcesarBloque(ObtenerSiguienteBloqueAProcesar(objArchivoCarga.Id), objArchivoCarga, readModel, readFilaModel))
        {
        }
        await FinalizarProcesoDeBloquesDeArchivo(objArchivoCarga);
    }

    public async Task ProcesarBloqueDeArchivoAsync(Guid idArchivoCarga, Guid idBloqueCarga)
    {
        var objArchivoCarga = _bloqueGenericRepository.GetById<ArchivoCarga>(idArchivoCarga);
        if (objArchivoCarga.Estado == EstadoCarga.PENDIENTE) 
        { 
            await IniciarProcesoDeBloquesDeArchivo(objArchivoCarga);
        }

        bool readModel = true;
        bool readFilaModel = false;
        var _ = await ProcesarBloque(ObtenerActualBloqueAProcesar(idBloqueCarga), objArchivoCarga, readModel, readFilaModel);

        if (VerficarSiEsUltimoBloqueAProcesar(idBloqueCarga))
        {
            await FinalizarProcesoDeBloquesDeArchivo(objArchivoCarga);
        }
    }

    private async Task<bool> ProcesarBloque(TBloque objBloqueEnProceso, ArchivoCarga objArchivoCarga, bool readModel, bool readFilaModel)
    {
        ContadoresProceso contadoresBloque = null;
        TBloque objBloqueDeBD = null; //para pruebas de verificación;

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

            #region Validacion preliminar de atributos
            int hijosInvalidos = 0;
            var arrayPropertiesFila = filaEnProceso.GetType().GetProperties();
            for (int f = 0; f < arrayPropertiesFila.Length; f++)
            {
                PropertyInfo pFila = arrayPropertiesFila[f];
                if (pFila.PropertyType.AssemblyQualifiedName.StartsWith(lstIdsPropiedadesListas[0]))
                {
                    object currentVal = pFila.GetValue(filaEnProceso);
                    foreach (var innerObject in (IEnumerable<object>)currentVal)
                    {
                        IValidable obj = (IValidable)innerObject;
                        var innerValidationResult = obj.ValidarAtributos();
                        (innerObject as FilaArchivoCarga).Evaluado = true;
                        (innerObject as FilaArchivoCarga).EsValido = innerValidationResult.Observaciones.Count > 0 ? false : true;
                        if (innerValidationResult.Observaciones.Count > 0) (innerObject as FilaArchivoCarga).Observaciones = innerValidationResult.Observaciones;
                        if ((innerObject as FilaArchivoCarga).EsValido == false) hijosInvalidos++;
                    }
                }
            }

            if (validationResult.EsValido && (hijosInvalidos > 0 || validationResult.Observaciones.Count > 0))
            {
                contadoresBloque.Evaluados++;
                contadoresBloque.EvaluadosObservados++;
                filaEnProceso.EsValido = false;
                filaEnProceso.Observaciones.AddRange(validationResult.Observaciones);
                continue;
            }
            #endregion

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

            #region Validacion result de atributos 
            int hijosInvalidosResult = 0;
            var arrayPropertiesFilaResult = filaEnProceso.GetType().GetProperties();
            for (int f = 0; f < arrayPropertiesFilaResult.Length; f++)
            {
                PropertyInfo pFila = arrayPropertiesFilaResult[f];
                if (pFila.PropertyType.AssemblyQualifiedName.StartsWith(lstIdsPropiedadesListas[0]))
                {
                    object currentVal = pFila.GetValue(filaEnProceso);
                    foreach (var innerObjectResult in (IEnumerable<object>)currentVal)
                    {
                        var resultObs = validationResult.InnerResults.Find(r => r.GroupKey == pFila.Name);
                        if (resultObs != null)
                        {
                            var obsList = resultObs.FilaObservaciones.FindAll(o => o.NroFila == (innerObjectResult as FilaArchivoCarga).NumeroFila);
                            foreach (var obs in obsList)
                            {
                                (innerObjectResult as FilaArchivoCarga).Observaciones.Add(obs.Observacion);
                                hijosInvalidosResult++;
                            }
                        }
                    }
                }
            }
            #endregion

            filaEnProceso.NumeroFila = objBloqueDeBD.FilaInicial + i;

            #region Asimilacion de resultados de validacion
            filaEnProceso.Evaluado = true;
            filaEnProceso.EsValido = validationResult.EsValido;
            if (hijosInvalidosResult > 0)
                filaEnProceso.EsValido = false;
            filaEnProceso.Observaciones.AddRange(validationResult.Observaciones);
            #endregion

            #region Contabilizar procesados, validados y observados
            if (filaEnProceso.Evaluado) { contadoresBloque.Evaluados++; }
            if (filaEnProceso.Evaluado && filaEnProceso.EsValido) { contadoresBloque.EvaluadosValidos++; }
            if (filaEnProceso.Evaluado && !filaEnProceso.EsValido) { contadoresBloque.EvaluadosObservados++; }
            #endregion
            bool enviarNotificacionProgreso = false;
            #region Invocacion a evento de progreso según frecuencia configurada

            if (objBloqueDeBD.CantidadTotalElementos < frecuenciaActualizacionAvanceProcesoMinimo)
            {
                enviarNotificacionProgreso = true;
            }
            else if (objBloqueDeBD.CantidadTotalElementos < frecuenciaActualizacionAvanceProcesoEstandar &&
                     filaEnProceso.NumeroFila % frecuenciaActualizacionAvanceProcesoMinimo == 0)
            {
                enviarNotificacionProgreso = true;
            }
            else if (filaEnProceso.NumeroFila % frecuenciaActualizacionAvanceProcesoEstandar == 0)
            {
                enviarNotificacionProgreso = true;
            }
            else if (filaEnProceso.NumeroFila == objArchivoCarga.CantidadTotalElementos)
            {
                enviarNotificacionProgreso = true;
            }

            if (enviarNotificacionProgreso)
            {
                await _seguimientoBloqueService.ProcesarMensajeProgresoAsync(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Progreso)
                {
                    IdArchivoCarga = objArchivoCarga.Id,
                    IdBloque = currentBloqueId,
                    IdEntidad = objArchivoCarga.IdEntidad,
                    CodigoEntidad = objArchivoCarga.CodigoEntidad,
                    ContadoresProceso = contadoresBloque
                });
            }
            #endregion
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

    private TBloque ObtenerActualBloqueAProcesar(Guid idBloqueCarga)
    {
        var objActualBloqueAProcesar = _bloqueGenericRepository.GetOne<TBloque>(x =>
                                                    x.Id == idBloqueCarga &&
                                                    x.Estado == EstadoCarga.PENDIENTE &&
                                                    x.EsActivo == true &&
                                                    x.EsEliminado == false);

        _logger.LogDebug($"Verificando si existen bloques por procesar...{objActualBloqueAProcesar != null}");

        return objActualBloqueAProcesar;
    }

    private bool VerficarSiEsUltimoBloqueAProcesar(Guid idCargaArchivo)
    {
        var esUltimoBloque = !_bloqueGenericRepository.Any<TBloque>(x =>
                                                        x.IdCarga == idCargaArchivo &&
                                                        (x.Estado != EstadoCarga.EVALUADO_CORRECTO && x.Estado != EstadoCarga.EVALUADO_CON_OBSERVACIONES) &&
                                                        x.EsActivo == true &&
                                                        x.EsEliminado == false);

        _logger.LogDebug($"Verificando si es ultimo bloque por procesar...{esUltimoBloque}");

        return esUltimoBloque;
    }
    private async Task IniciarProcesoDeBloquesDeArchivo(ArchivoCarga objArchivoCarga)
    {
        _logger.LogDebug($"Iniciando el proceso del archivo de carga id = {objArchivoCarga.Id}");
        #region 1) Marcando el archivo como "en evaluación"
        var updArchivoEnEvaluacion = Builders<ArchivoCarga>.Update
                                                .Set(b => b.Estado, EstadoCarga.EN_EVALUACION)
                                                .Set(b => b.FechaEvaluacionInicio, DateTime.Now)
                                                .Set(b => b.UsuarioModificacion, _usuarioAutor)
                                                .Set(b => b.FechaModificacion, DateTime.Now);

        _bloqueGenericRepository.UpdateOne(objArchivoCarga, updArchivoEnEvaluacion);
        #endregion

        #region 2) Generando evento "Inicio de proceso"
        await _seguimientoBloqueService.ProcesarMensajeProgresoAsync(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Inicio)
        {
            IdArchivoCarga = objArchivoCarga.Id,
            IdEntidad = objArchivoCarga.IdEntidad,
            CodigoEntidad = objArchivoCarga.CodigoEntidad,
            ContadoresProceso = new ContadoresProceso()
            {
                TotalElementos = objArchivoCarga.CantidadTotalElementos
            }
        });
        #endregion
    }
    private async Task FinalizarProcesoDeBloquesDeArchivo(ArchivoCarga objArchivoCarga)
    {
        _logger.LogDebug($"Finalizando el proceso del archivo de carga id = {objArchivoCarga.Id}");
        #region 1) Sumarizando contadores de archivo y marcando el registro como "Procesado"

        ContadoresProceso contadorArchivo = null;
        SumarizarContadoresDeBloquesDeArchivo(objArchivoCarga.Id, out contadorArchivo);

        var updFinProcesamiento = Builders<ArchivoCarga>.Update
            .Set(b => b.FechaEvaluacionFin, DateTime.Now)
            .Set(b => b.FechaModificacion, DateTime.Now)
            .Set(b => b.IpModificacion, _ipOrigen)
            .Set(b => b.UsuarioModificacion, _usuarioAutor);

        _bloqueGenericRepository.UpdateOne(objArchivoCarga, updFinProcesamiento);

        #endregion
        #region 2) Generando evento Finalizacion de proceso
        await _seguimientoBloqueService.ProcesarMensajeProgresoAsync(new ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo.Fin)
        {
            IdArchivoCarga = objArchivoCarga.Id,
            IdEntidad = objArchivoCarga.IdEntidad,
            CodigoEntidad = objArchivoCarga.CodigoEntidad,
            ContadoresProceso = contadorArchivo ?? new ContadoresProceso()
        });
        #endregion
    }
}
