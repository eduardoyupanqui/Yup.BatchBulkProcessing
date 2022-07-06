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
using Yup.Soporte.Api.Application.Services.Factories;
using Yup.Soporte.Api.Settings;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CrearCargaServicioExternoCommandHandler : IRequestHandler<CrearCargaServicioExternoCommand<DatosPersonaRequest>, GenericResult<Guid>>
{
    private readonly ISoporteIntegrationEventService _soporteIntegrationEventService;
    private readonly ICargaServicioExternoRegistroService<DatosPersonaRequest> _registroCargaServicioExternoService;
    private readonly ICargaServicioExternoCommandValidator<DatosPersonaRequest> _crearCargaServicioExternoCommandValidator;
    private readonly IntegracionEventGeneratorFactory _integracionEventGenerator;
    private readonly CargaMasivaSettings _cargaMasivaSettings;

    public CrearCargaServicioExternoCommandHandler(
        ISoporteIntegrationEventService soporteIntegrationEventService,
        ICargaServicioExternoRegistroService<DatosPersonaRequest> registroCargaServicioExternoService,
        ICargaServicioExternoCommandValidator<DatosPersonaRequest> crearCargaServicioExternoCommandValidator,
        IntegracionEventGeneratorFactory integracionEventGenerator,
        CargaMasivaSettings cargaMasivaSettings
        )
    {
        _soporteIntegrationEventService = soporteIntegrationEventService ?? throw new ArgumentNullException(nameof(soporteIntegrationEventService));
        _registroCargaServicioExternoService = registroCargaServicioExternoService ?? throw new ArgumentNullException(nameof(registroCargaServicioExternoService));
        _crearCargaServicioExternoCommandValidator = crearCargaServicioExternoCommandValidator ?? throw new ArgumentNullException(nameof(crearCargaServicioExternoCommandValidator));
        _integracionEventGenerator = integracionEventGenerator ?? throw new ArgumentNullException(nameof(integracionEventGenerator));

        _cargaMasivaSettings = cargaMasivaSettings;
    }

    public async Task<GenericResult<Guid>> Handle(CrearCargaServicioExternoCommand<DatosPersonaRequest> request, CancellationToken cancellationToken)
    {
        var result = new GenericResult<Guid>();

        ID_TBL_FORMATOS_CARGA tipoCargaActual = request.IdTblTipoCarga;
        var tipoCargaSettings = _cargaMasivaSettings.GetSettingsPorTipoCarga(tipoCargaActual);

        #region Validacion especializada
        result = await _crearCargaServicioExternoCommandValidator.Validate(request, completarDatosDescriptivos: true);
        if (result.HasErrors) { return result; }
        #endregion

        #region Registro
        var registroResult = await _registroCargaServicioExternoService.RegistrarCargaYBloques(request);
        if (registroResult.HasErrors)
        {
            result.AddError(registroResult.Messages.FirstOrDefault().Message);
            return result;
        }
        #endregion
        result.DataObject = registroResult.DataObject.Item1;

        #region Emisión de Evento de Integración
        var integracionEventGenerator = _integracionEventGenerator.Create(tipoCargaActual);
        if (!tipoCargaSettings.ProcesarPorBloque)
        {
            var @genericEvent = integracionEventGenerator.GenerarEventoIntegracion(registroResult.DataObject.Item1);
            if (@genericEvent != null)
            {
                await _soporteIntegrationEventService.SaveEventAndSoporteContextChangesAsync(@genericEvent);
                await _soporteIntegrationEventService.PublishThroughEventBusAsync(@genericEvent);
            }
        }
        else
        {
            var @genericEvents = registroResult.DataObject.Item2.Select(guidBloque => integracionEventGenerator.GenerarBloqueEventoIntegracion(registroResult.DataObject.Item1, guidBloque));            
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

        return result;
    }
}
