using System;
using Yup.Soporte.Api.Application.IntegrationEvents;

namespace Yup.Soporte.Api.Application.IntegrationEvents.Events;

public class ProcesarDatosPersonaMasivaIntegrationEvent : IntegrationEvent
{
    public Guid GuidArchivo { get; }

    public ProcesarDatosPersonaMasivaIntegrationEvent(Guid guidArchivo)
    {
        GuidArchivo = guidArchivo;
    }
}
