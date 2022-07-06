using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Yup.Soporte.Api.Infrastructure.Services;

namespace Yup.Soporte.Api.Application.IntegrationEvents;

public class SoporteIntegrationEventService : ISoporteIntegrationEventService
{
    private readonly ILogger _logger;
    private readonly IEventBus _eventBus;
    public SoporteIntegrationEventService(
         ILogger<SoporteIntegrationEventService> logger,
         IEventBus eventBus)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus;
    }
    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
            await _eventBus.Publish(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
        }
    }

    public Task SaveEventAndSoporteContextChangesAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("----- SoporteIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);
        return Task.CompletedTask;
    }
}
