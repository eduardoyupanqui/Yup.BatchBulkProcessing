using System.Threading.Tasks;

namespace Yup.Soporte.Api.Application.IntegrationEvents
{
    public interface ISoporteIntegrationEventService
    {
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
        Task SaveEventAndSoporteContextChangesAsync(IntegrationEvent evt);
    }
}
