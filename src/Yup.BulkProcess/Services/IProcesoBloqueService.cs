using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yup.BulkProcess;

public interface IProcesoBloqueService
{
    void SetAuditoriaInfo(string usuarioAutor, string ipOrigen, string hostNameOrigen);
    Func<ProcesoArchivoCargaEventArgs, Task> InicioProcesoAsync { set; }
    Func<ProcesoArchivoCargaEventArgs, Task> ProgresoProcesoAsync { set; }
    Func<ProcesoArchivoCargaEventArgs, Task> FinProcesoAsync { set; }
    Task ProcesarBloquesDeArchivoAsync(Guid idArchivoCarga);
}
