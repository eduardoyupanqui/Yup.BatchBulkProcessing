using System.Text.Json.Serialization;

namespace Yup.Student.BulkProcess.Application.IntegrationEvents;

public class IntegrationEvent
{
    public IntegrationEvent() { }
    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate) { }
    public Guid Id { get; }
    public DateTime CreationDate { get; }
}
