using System.Text.Json.Serialization;

namespace Yup.Soporte.Api.Application.Commands;

public abstract class AuditoriaCommand
{
    [JsonIgnore]
    public Guid UsuarioRegistro { get; set; }
    [JsonIgnore]
    public DateTime FechaRegistro { get; set; }
    [JsonIgnore]
    public string IpRegistro { get; set; }
}
