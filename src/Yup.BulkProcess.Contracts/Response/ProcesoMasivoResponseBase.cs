using System.Collections.Generic;

namespace Yup.BulkProcess.Contracts.Response;

public abstract class ProcesoMasivoResponseBase
{
    public ProcesoMasivoResponseBase()
    {
        Observaciones = new List<string>();
    }
    public int NumeroElemento { get; set; } //Numero de fila (Excel), ínice de elemento en el listado (llamada de servicio externo)
    public bool Evaluado { get; set; }
    public bool Registrado { get; set; }
    public bool EsValido { get; set; }
    public List<string> Observaciones { get; set; }
}
