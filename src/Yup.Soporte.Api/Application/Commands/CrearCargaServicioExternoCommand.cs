using Yup.BulkProcess.Contracts.Request;
using Yup.Enumerados;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Core;
using Yup.Soporte.Api.Application.Services.Factories;
using Yup.Soporte.Domain.SeedworkMongoDB;

namespace Yup.Soporte.Api.Application.Commands;

public class CrearCargaServicioExternoCommand : CrearCargaCommand
{
    public CrearCargaServicioExternoCommand() : base()
    {
        IdOrigenCarga = (int)OrigenCarga.SERVICIO_EXTERNO;
    }
    public IEnumerable<Dictionary<string,string>> Elementos { get; set; }

    public override int CantidadRegistrosTotal {
        get { if (Elementos == null) return 0; else return Elementos.Count(); }
        set { base.CantidadRegistrosTotal = value; }
    }

    public class CrearCargaServicioExternoCommandHandler : IRequestHandler<CrearCargaServicioExternoCommand, GenericResult<Guid>>
    {
        private readonly ISoporteIntegrationEventService _soporteIntegrationEventService;
        private readonly RegistroCargaServicioExternoServiceFactory _registroCargaServicioExternoService;
        private readonly CrearCargaServicioExternoCommandValidatorFactory _crearCargaServicioExternoCommandValidator;
        private readonly IntegracionEventGeneratorFactory _integracionEventGenerator;

        public CrearCargaServicioExternoCommandHandler(
            ISoporteIntegrationEventService soporteIntegrationEventService,
            RegistroCargaServicioExternoServiceFactory registroCargaServicioExternoService,
            CrearCargaServicioExternoCommandValidatorFactory crearCargaServicioExternoCommandValidator,
            IntegracionEventGeneratorFactory integracionEventGenerator
            )
        {
            _soporteIntegrationEventService = soporteIntegrationEventService ?? throw new ArgumentNullException(nameof(soporteIntegrationEventService));
            _registroCargaServicioExternoService = registroCargaServicioExternoService ?? throw new ArgumentNullException(nameof(registroCargaServicioExternoService));
            _crearCargaServicioExternoCommandValidator = crearCargaServicioExternoCommandValidator ?? throw new ArgumentNullException(nameof(crearCargaServicioExternoCommandValidator));
            _integracionEventGenerator = integracionEventGenerator ?? throw new ArgumentNullException(nameof(integracionEventGenerator));
        }

        public async Task<GenericResult<Guid>> Handle(CrearCargaServicioExternoCommand request, CancellationToken cancellationToken)
        {
            var result = new GenericResult<Guid>();

            IntegrationEvent @genericEvent = null;
            ID_TBL_FORMATOS_CARGA tipoCargaActual = request.IdTblTipoCarga;

            #region Validacion especializada
            result = await _crearCargaServicioExternoCommandValidator.Create(tipoCargaActual).Validate(request, completarDatosDescriptivos: true);
            if (result.HasErrors) { return result; }
            #endregion

            #region Registro
            var registroResult = await _registroCargaServicioExternoService.Create(tipoCargaActual).RegistrarCargaYBloques(request);
            if (registroResult.HasErrors)
            {
                return registroResult;
            }
            #endregion

            #region Emisión de Evento de Integración
            @genericEvent = _integracionEventGenerator.Create(tipoCargaActual).GenerarEventoIntegracion(registroResult.DataObject);
            result.DataObject = registroResult.DataObject;

            if (@genericEvent != null)
            {
                await _soporteIntegrationEventService.SaveEventAndSoporteContextChangesAsync(@genericEvent);
                await _soporteIntegrationEventService.PublishThroughEventBusAsync(@genericEvent);
            }

            #endregion

            return result;
        }
    }

}
