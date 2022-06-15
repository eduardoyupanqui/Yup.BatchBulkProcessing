using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yup.BulkProcess;

public interface ISeguimientoProcesoBloqueService
{
    Func<SeguimientoProcesoArchivoEventArgs, Task> StatusUpdateAsync { set; }
    Func<SeguimientoProcesoArchivoEventArgs, Task> ProcessCompletedAsync { set; }
    Task ProcesarMensajeProgresoAsync(ProcesoArchivoCargaEventArgs eventArgs);
}
