using Yup.Soporte.Api.Application.IntegrationEvents.Events;
using Yup.Soporte.Api.Application.IntegrationEvents;
using Yup.Soporte.Api.Application.Services.Interfaces;
using System;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class IntegrationEventGenerator : IIntegrationArchivoCargaService<ProcesarDatosPersonaMasivaIntegrationEvent>
{
    public IntegrationEvent GenerarEventoIntegracion(params object[] parametros)
    {
        var guidArchivo = (Guid)parametros[0];
        return new ProcesarDatosPersonaMasivaIntegrationEvent(guidArchivo);
    }
    public IntegrationEvent GenerarBloqueEventoIntegracion(params object[] parametros)
    {
        var guidArchivo = (Guid)parametros[0];
        var guidBloque = (Guid)parametros[1];
        return new ProcesarDatosPersonaBloqueIntegrationEvent(guidArchivo, guidBloque);
    }
}
