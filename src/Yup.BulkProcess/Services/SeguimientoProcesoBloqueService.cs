using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yup.BulkProcess;

public class SeguimientoProcesoBloqueService : ISeguimientoProcesoBloqueService
{
    public Func<SeguimientoProcesoArchivoEventArgs, Task> StatusUpdateAsync { private get; set; }
    public Func<SeguimientoProcesoArchivoEventArgs, Task> ProcessCompletedAsync { private get; set; }
    public SeguimientoProcesoBloqueService()
    {

    }
    public Task ProcesarMensajeProgresoAsync(ProcesoArchivoCargaEventArgs eventArgs)
    {
        return Task.CompletedTask;
    }
}
