namespace Yup.Soporte.Api.Application.Commands;

public abstract class AuditoriaCommand
{
    public Guid UsuarioRegistro { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string IpRegistro { get; set; }
}
