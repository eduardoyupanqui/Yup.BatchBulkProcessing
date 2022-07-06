using MediatR;
using Yup.Core;
using Yup.Enumerados;
using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Soporte.Api.Application.Services.Factories;
using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.SeedworkMongoDB;

namespace Yup.Soporte.Api.Application.Commands;

public class CrearCargaArchivoExcelCommand: CrearCargaCommand
{
    public CrearCargaArchivoExcelCommand()
    {
        base.IdOrigenCarga = (int)OrigenCarga.ARCHIVO_EXCEL;
        FlagsPermisos = new CrearCargaFlagsPermisos();
    }
    //Solo archivos
    public string ArchivoRuta { get; set; }
    public string ArchivoNombre { get; set; }
    public string ArchivoExtension { get; set; }
    public long ArchivoTamanio { get; set; }
    public bool ArchivoBasadoEnPlantilla { get; set; }
    public bool ArchivoTieneFirmaDigital { get; set; }

    public string DatosAdicionales { get; set; } //?
    public CrearCargaFlagsPermisos FlagsPermisos { get; set; }
    public class CrearCargaFlagsPermisos
    {
        public bool EstadoMatrizEntidadEstudiante { get; set; }

    }

    public class CrearCargaArchivoExcelCommandHandler : IRequestHandler<CrearCargaArchivoExcelCommand, GenericResult<Guid>>
    {
        private readonly ILogger _logger;
        private readonly ISoporteIntegrationEventService _soporteIntegrationEventService;
        private readonly RegistroCargaArchivoExcelServiceFactory _registroCargaArchivoExcelServiceFactory;
        private readonly CrearCargaArchivoExcelCommandValidatorFactory _crearCargaArchivoExcelCommandValidatorFactory;
        private readonly IntegracionEventGeneratorFactory _integracionEventGenerator;
        private readonly CargaMasivaSettings _cargaMasivaSettings;
        public CrearCargaArchivoExcelCommandHandler(
            ILogger<CrearCargaArchivoExcelCommandHandler> logger, 
            ISoporteIntegrationEventService soporteIntegrationEventService,
            RegistroCargaArchivoExcelServiceFactory registroCargaArchivoExcelServiceFactory,
            CrearCargaArchivoExcelCommandValidatorFactory crearCargaArchivoExcelCommandValidatorFactory,
            IntegracionEventGeneratorFactory integracionEventGenerator,
            CargaMasivaSettings cargaMasivaSettings
            )
        {
            _logger = logger;
            _soporteIntegrationEventService = soporteIntegrationEventService;
            _registroCargaArchivoExcelServiceFactory = registroCargaArchivoExcelServiceFactory;
            _crearCargaArchivoExcelCommandValidatorFactory = crearCargaArchivoExcelCommandValidatorFactory;
            _integracionEventGenerator = integracionEventGenerator;
            _cargaMasivaSettings = cargaMasivaSettings;
        }
        public async Task<GenericResult<Guid>> Handle(CrearCargaArchivoExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<Guid>();
            try
            {
                ID_TBL_FORMATOS_CARGA tipoCargaActual = request.IdTblTipoCarga;
                var tipoCargaSettings = _cargaMasivaSettings.GetSettingsPorTipoCarga(tipoCargaActual);

                #region Validacion especializada
                var registroCargaCommandValidator = _crearCargaArchivoExcelCommandValidatorFactory.Create(tipoCargaActual);
                result = await registroCargaCommandValidator.Validate(request, true);
                if (result.HasErrors) { return result; }
                #endregion

                #region Registro
                var registroCargaService = _registroCargaArchivoExcelServiceFactory.Create(tipoCargaActual);
                var registroResult = await registroCargaService.RegistrarCargaYBloques(request);
                if (registroResult.HasErrors)
                {
                    result.AddError(registroResult.Messages.FirstOrDefault().Message);
                    return result;
                }
                #endregion

                var (idArchivoCarga, idsBloque) = registroResult.DataObject;
                result.DataObject = idArchivoCarga;

                #region Emisión de Evento de Integración
                var integrationEventGenerator = _integracionEventGenerator.Create(tipoCargaActual);
                if (!tipoCargaSettings.ProcesarPorBloque)
                {
                    var @genericEvent = integrationEventGenerator.GenerarEventoIntegracion(idArchivoCarga, request.DatosAdicionales);
                    if (@genericEvent != null)
                    {
                        await _soporteIntegrationEventService.SaveEventAndSoporteContextChangesAsync(@genericEvent);
                        await _soporteIntegrationEventService.PublishThroughEventBusAsync(@genericEvent);
                    }
                }
                else
                {
                    var @genericEvents = idsBloque.Select(idBloque => integrationEventGenerator.GenerarBloqueEventoIntegracion(idArchivoCarga, idBloque));
                    if (@genericEvents != null)
                    {
                        foreach (var @genericEvent in @genericEvents)
                        {
                            await _soporteIntegrationEventService.SaveEventAndSoporteContextChangesAsync(@genericEvent);
                            await _soporteIntegrationEventService.PublishThroughEventBusAsync(@genericEvent);
                        }
                    }
                }
                
                #endregion
            }
            catch (Exception ex)
            {
                var errorMsg = "Ocurrió un error en en registro del archivo. por favor reintente en un momento.";
                _logger.LogError(ex, errorMsg);
                result.AddError(errorMsg);
            }

            return result;
        }
    }
}

