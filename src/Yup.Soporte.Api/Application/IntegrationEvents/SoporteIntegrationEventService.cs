using Microsoft.Extensions.Logging;
using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Yup.Soporte.Api.Application.IntegrationEvents;

public class SoporteIntegrationEventService : ISoporteIntegrationEventService
{
    private readonly ILogger _logger;

    public SoporteIntegrationEventService(
         ILogger<SoporteIntegrationEventService> logger)

    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
    {
        try
        {
            _logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
            //_eventBus.Publish(evt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})", evt.Id, Program.AppName, evt);
        }
    }

    public async Task SaveEventAndSoporteContextChangesAsync(IntegrationEvent evt)
    {
        _logger.LogInformation("----- SoporteIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);
    }
}
