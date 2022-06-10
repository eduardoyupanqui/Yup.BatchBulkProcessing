using System.Text.Json.Serialization;

namespace Yup.Soporte.Api.Application.IntegrationEvents;

public class IntegrationEvent
{
    public IntegrationEvent() { }
    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate) { }
    public Guid Id { get; }
    public DateTime CreationDate { get; }
}
