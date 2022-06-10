namespace Yup.Soporte.Api.Settings;

public class CargaMasivaSettings
{
    public int CantidadMaximaRegistros { get; set; }
    public string[] ExtensionesArchivoPermitidas { get; set; }
    public int TamanoMaximoDeArchivoEnMB { get; set; }
    public int FilaInicialLecturaExcel { get; set; }
    public int VisualizacionCantidadMinimaRegistros { get; set; }

    public string RutaBaseArchivos { get; set; } //PathServerFile:Carga:Archivo

}
