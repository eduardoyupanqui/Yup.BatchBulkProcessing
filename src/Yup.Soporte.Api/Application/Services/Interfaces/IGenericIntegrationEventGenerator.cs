using Yup.Soporte.Api.Application.IntegrationEvents;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

public interface IGenericIntegrationEventGenerator
{
    IntegrationEvent GenerarEventoIntegracion(params object[] parametros);
}

public interface IIntegrationArchivoCargaService<TCustomIntegrationEvent> : IGenericIntegrationEventGenerator
    where TCustomIntegrationEvent : IntegrationEvent
{
}
