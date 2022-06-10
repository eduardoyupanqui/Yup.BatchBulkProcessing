using Yup.Enumerados;
using MediatR;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yup.BulkProcess.Contracts.Request;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CrearCargaServicioExternoCommandHandler : IRequestHandler<CrearCargaServicioExternoCommand<DatosPersonaRequest>, GenericResult<Guid>>
{
    private readonly ISoporteIntegrationEventService _soporteIntegrationEventService;
    private readonly ICargaServicioExternoRegistroService<DatosPersonaRequest> _registroCargaServicioExternoService;
    private readonly ICargaServicioExternoCommandValidator<DatosPersonaRequest> _crearCargaServicioExternoCommandValidator;
    private readonly Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator> _integracionEventGenerator;

    public CrearCargaServicioExternoCommandHandler(
        ISoporteIntegrationEventService soporteIntegrationEventService,
        ICargaServicioExternoRegistroService<DatosPersonaRequest> registroCargaServicioExternoService,
        ICargaServicioExternoCommandValidator<DatosPersonaRequest> crearCargaServicioExternoCommandValidator,
        Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator> integracionEventGenerator
        )
    {
        _soporteIntegrationEventService = soporteIntegrationEventService ?? throw new ArgumentNullException(nameof(soporteIntegrationEventService));
        _registroCargaServicioExternoService = registroCargaServicioExternoService ?? throw new ArgumentNullException(nameof(registroCargaServicioExternoService));
        _crearCargaServicioExternoCommandValidator = crearCargaServicioExternoCommandValidator ?? throw new ArgumentNullException(nameof(crearCargaServicioExternoCommandValidator));
        _integracionEventGenerator = integracionEventGenerator ?? throw new ArgumentNullException(nameof(integracionEventGenerator));
    }

    public async Task<GenericResult<Guid>> Handle(CrearCargaServicioExternoCommand<DatosPersonaRequest> request, CancellationToken cancellationToken)
    {
        var result = new GenericResult<Guid>();

        IntegrationEvent @genericEvent = null;
        ID_TBL_FORMATOS_CARGA tipoCargaActual = request.IdTblTipoCarga;

        #region Validacion especializada
        result = await _crearCargaServicioExternoCommandValidator.Validate(request, completarDatosDescriptivos: true);
        if (result.HasErrors) { return result; }
        #endregion

        #region Registro
        var registroResult = await _registroCargaServicioExternoService.RegistrarCargaYBloques(request);
        if (registroResult.HasErrors)
        {
            return registroResult;
        }
        #endregion

        #region Emisión de Evento de Integración
        @genericEvent = _integracionEventGenerator(tipoCargaActual).GenerarEventoIntegracion(registroResult.DataObject);
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
