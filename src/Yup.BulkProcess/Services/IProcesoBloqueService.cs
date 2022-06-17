using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Validation;

namespace Yup.BulkProcess;

public interface IProcesoBloqueService<TBloque, TFila, TFilaModel>
                                                      where TBloque : BloqueCarga<TFila>
                                                      where TFila : FilaArchivoCarga
                                                      where TFilaModel : IValidable
{
    void SetAuditoriaInfo(string usuarioAutor, string ipOrigen, string hostNameOrigen);
    void SetValidator(IValidator<TFilaModel> modelValidator);
    Func<ProcesoArchivoCargaEventArgs, Task> InicioProcesoAsync { set; }
    Func<ProcesoArchivoCargaEventArgs, Task> ProgresoProcesoAsync { set; }
    Func<ProcesoArchivoCargaEventArgs, Task> FinProcesoAsync { set; }
    Task ProcesarBloquesDeArchivoAsync(Guid idArchivoCarga);
}
