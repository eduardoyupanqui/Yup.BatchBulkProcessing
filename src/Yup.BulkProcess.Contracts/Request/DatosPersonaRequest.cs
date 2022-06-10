using System;
using System.Collections.Generic;
using System.Text;

namespace Yup.BulkProcess.Contracts.Request;

public class DatosPersonaRequest : ProcesoMasivoRequestBase
{
    public string TipoDocumento { get; set; }
    public string NroDocumento { get; set; }
    public string LenguaNativa { get; set; }
    public string IdiomaExtranjero { get; set; }
    public string CondicionDiscapacidad { get; set; }
    public string CodigoORCID { get; set; }
    //public string CorreoPersonal { get; set; }
    //public string CorreoInstitucional { get; set; }
    public string UbigeoDomicilio { get; set; }
}
