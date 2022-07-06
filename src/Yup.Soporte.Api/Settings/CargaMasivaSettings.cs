using Yup.Enumerados;

namespace Yup.Soporte.Api.Settings;

public class CargaMasivaSettings
{
    public int CantidadMaximaRegistros { get; set; }
    public string[] ExtensionesArchivoPermitidas { get; set; }
    public int TamanoMaximoDeArchivoEnMB { get; set; }
    public int FilaInicialLecturaExcel { get; set; }
    public int VisualizacionCantidadMinimaRegistros { get; set; }

    public string RutaBaseArchivos { get; set; } //PathServerFile:Carga:Archivo

    public Dictionary<string, SettingsPorTipoCarga> SettingsPorTipoCarga { get; set; }

    public SettingsPorTipoCarga GetSettingsPorTipoCarga(ID_TBL_FORMATOS_CARGA tipoCarga)
    {
        if (tipoCarga == ID_TBL_FORMATOS_CARGA.NONE)
            throw new ArgumentException("No se puede acceder a una configuracion de un tipo de carga NONE");

        if (SettingsPorTipoCarga == null || !SettingsPorTipoCarga.ContainsKey(tipoCarga.ToString()))
            return new SettingsPorTipoCarga();

        return SettingsPorTipoCarga[tipoCarga.ToString()];        
    }
}

public class SettingsPorTipoCarga
{
    public bool ProcesarPorBloque { get; set; } = false;
}