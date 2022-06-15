using System;
using System.Collections.Generic;
using System.Text;

namespace Yup.BulkProcess;

public class ProcesoArchivoCargaEventArgs
{
    public ProcesoArchivoCargaEventArgs(TipoEventoProcesoArchivo tipoEvento)
    {
        TipoEvento = tipoEvento;
        ContadoresProceso = new ContadoresProceso();
    }
    public int IdEntidad { get; set; }
    public string CodigoEntidad { get; set; }
    public TipoEventoProcesoArchivo TipoEvento { get; private set; }
    public Guid IdArchivoCarga { get; set; }
    public Guid IdBloque { get; set; }
    public ContadoresProceso ContadoresProceso { get; set; }
}
public class ContadoresProceso
{
    /// <summary>
    /// Cantidad de total de registros del archivo o bloque
    /// </summary>
    public int TotalElementos { get; set; }
    /// <summary>
    /// Cantidad de elementos que fueron evaluados
    /// </summary>
    public int Evaluados { get; set; }

    /// <summary>
    /// Cantidad de elementos que no pasaron la validación
    /// </summary>
    public int EvaluadosObservados { get; set; }
    /// <summary>
    /// Cantidad de elementos que pasaron la validación
    /// </summary>
    public int EvaluadosValidos { get; set; }
    /// <summary>
    /// Cantidad de elementos que fueron registrados (migrados) luego de pasar la validación
    /// </summary>
    public int RegistradosValidos { get; set; }

    /// <summary>
    /// Cantidad de elementos que pasaron la validación, pero presentaron errores en el registro
    /// </summary>
    public int RegistradosFallidos { get; set; }
}
public enum TipoEventoProcesoArchivo
{
    Inicio,
    Progreso,
    Fin
}

public class SeguimientoProcesoArchivoEventArgs
{
    public SeguimientoProcesoArchivoEventArgs()
    {
        ContadoresProceso = new ContadoresProceso();
    }
    public int IdEntidad { get; set; }
    public string CodigoEntidad { get; set; }
    public Guid IdArchivoCarga { get; set; }
    public ContadoresProceso ContadoresProceso { get; set; }

}
