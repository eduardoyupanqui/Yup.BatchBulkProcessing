using System;

namespace Yup.Student.BulkProcess.Application.IntegrationEvents.Events;

public class ProcesarDatosPersonaMasivaIntegrationEvent : IntegrationEvent
{
    public Guid GuidArchivo { get; }

    public ProcesarDatosPersonaMasivaIntegrationEvent(Guid guidArchivo)
    {
        GuidArchivo = guidArchivo;
    }
}
