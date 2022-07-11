using System;

namespace Yup.Student.BulkProcess.Application.IntegrationEvents.Events;

public class ProcesarDatosPersonaBloqueIntegrationEvent : IntegrationEvent
{
    public Guid GuidArchivo { get; }
    public Guid GuidBloque { get; }

    public ProcesarDatosPersonaBloqueIntegrationEvent(Guid guidArchivo, Guid guidBloque)
    {
        GuidArchivo = guidArchivo;
        GuidBloque = guidBloque;
    }
}
