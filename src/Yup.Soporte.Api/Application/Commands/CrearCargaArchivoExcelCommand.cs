using MediatR;
using Yup.Core;
using Yup.Enumerados;
using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Soporte.Api.Application.Services.Interfaces;
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
        private readonly Func<ID_TBL_FORMATOS_CARGA, ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>> _registroCargaArchivoExcelServiceFactory;
        private readonly Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaArchivoExcelCommand>> _crearCargaArchivoExcelCommandValidatorFactory;
        private readonly Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator> _integracionEventGenerator;

        public CrearCargaArchivoExcelCommandHandler(
            ILogger<CrearCargaArchivoExcelCommandHandler> logger, 
            ISoporteIntegrationEventService soporteIntegrationEventService,
            Func<ID_TBL_FORMATOS_CARGA, ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>> registroCargaArchivoExcelServiceFactory, 
            Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaArchivoExcelCommand>> crearCargaArchivoExcelCommandValidatorFactory, 
            Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator> integracionEventGenerator 
            )
        {
            _logger = logger;
            _soporteIntegrationEventService = soporteIntegrationEventService;
            _registroCargaArchivoExcelServiceFactory = registroCargaArchivoExcelServiceFactory;
            _crearCargaArchivoExcelCommandValidatorFactory = crearCargaArchivoExcelCommandValidatorFactory;
            _integracionEventGenerator = integracionEventGenerator;
        }
        public async Task<GenericResult<Guid>> Handle(CrearCargaArchivoExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<Guid>();
            try
            {
                IntegrationEvent @genericEvent = null;
                ID_TBL_FORMATOS_CARGA tipoCargaActual = request.IdTblTipoCarga;

                #region Validacion especializada
                var registroCargaCommandValidator = _crearCargaArchivoExcelCommandValidatorFactory(tipoCargaActual);
                result = await registroCargaCommandValidator.Validate(request, true);
                if (result.HasErrors) { return result; }
                #endregion

                #region Registro
                var registroCargaService = _registroCargaArchivoExcelServiceFactory(tipoCargaActual);
                var registroResult = await registroCargaService.RegistrarCargaYBloques(request);
                if (registroResult.HasErrors)
                {
                    return registroResult;
                }
                #endregion

                #region Emisión de Evento de Integración
                @genericEvent = _integracionEventGenerator(tipoCargaActual).GenerarEventoIntegracion(registroResult.DataObject, request.DatosAdicionales);
                result.DataObject = registroResult.DataObject;

                if (@genericEvent != null)
                {
                    await _soporteIntegrationEventService.SaveEventAndSoporteContextChangesAsync(@genericEvent);
                    await _soporteIntegrationEventService.PublishThroughEventBusAsync(@genericEvent);
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

