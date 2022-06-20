namespace Yup.Student.BulkProcess.Application.IntegrationEvents.Events;

public class EnviarNotificacionIntegrationEvent : IntegrationEvent
{
    public string CodigoEntidad { get; private set; }
    public string CodigoPlantilla { get; private set; }
    public Dictionary<string, string> ParametrosPlantilla { get; private set; }

    public EnviarNotificacionIntegrationEvent(string codigoEntidad, string codigoPlantilla, Dictionary<string, string> parametrosPlantilla)
    {
        CodigoEntidad = codigoEntidad;
        CodigoPlantilla = codigoPlantilla;
        ParametrosPlantilla = parametrosPlantilla;
    }
}