namespace Yup.Student.BulkProcess.Application.IntegrationEvents.Events;

public class ProcesoCargaStatusIntegrationEvent : IntegrationEvent
{
    public string IdEntidad { get; }
    public System.Guid ProcesoId { get; }
    public int Total { get; }
    public int Evaluados { get; }
    public int EvaluadosValidos { get; }
    public int EvaluadosObservados { get; }
    public int EstadoProceso { get; }

    public ProcesoCargaStatusIntegrationEvent(string idEntidad,
                                              System.Guid procesoId,
                                              int total,
                                              int evaluados,
                                              int evaluadosValidos,
                                              int evaluadosObservados
                                              )
    {
        IdEntidad = idEntidad;
        ProcesoId = procesoId;
        Total = total;
        Evaluados = evaluados;
        EvaluadosValidos = evaluadosValidos;
        EvaluadosObservados = evaluadosObservados;
    }
    public ProcesoCargaStatusIntegrationEvent(string idEntidad,
                                              System.Guid procesoId,
                                              int estadoProceso
                                             )
    {
        IdEntidad = idEntidad;
        ProcesoId = procesoId;
        EstadoProceso = estadoProceso;
    }
}
