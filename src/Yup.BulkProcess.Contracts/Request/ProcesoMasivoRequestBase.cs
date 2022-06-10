namespace Yup.BulkProcess.Contracts.Request;

public abstract class ProcesoMasivoRequestBase
{
    public int NumeroElemento { get; set; } //Numero de fila (Excel), ínice de elemento en el listado (llamada de servicio externo)
}
