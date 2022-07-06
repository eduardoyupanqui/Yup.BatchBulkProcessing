using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Soporte.Api.Infrastructure.Services;

namespace Yup.Soporte.Api.Infrastructure.Fakes;

public class FakeEventBus : IEventBus
{
    public FakeEventBus()
    {

    }

    public Task Publish(IntegrationEvent @event)
    {
        return Task.CompletedTask;
    }
}
