using System;
using System.Collections.Generic;

namespace Yup.BulkProcess;

public interface IMantenimientoProcesoService
{
    void SetAuditoriaInfo(string usuarioAutor, string ipOrigen, string hostNameOrigen);
    void RehabilitarArchivo(Guid idArchivoCarga);
    bool ActualizarFilasDeArchivo(Guid idArchivoCarga, IEnumerable<FilaArchivoCargaUpdate> updates);
    bool ActualizarFilasDeArchivo(Guid idArchivoCarga, IEnumerable<FilaArchivoCargaUpdate> updates, out ContadoresProceso contadoresArchivo);
}
