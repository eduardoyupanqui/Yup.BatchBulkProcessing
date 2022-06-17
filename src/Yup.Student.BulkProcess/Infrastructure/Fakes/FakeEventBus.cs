using Yup.Student.BulkProcess.Application.IntegrationEvents;
using Yup.Student.BulkProcess.Infrastructure.Services;

namespace Yup.Student.BulkProcess.Infrastructure.Fakes;

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
