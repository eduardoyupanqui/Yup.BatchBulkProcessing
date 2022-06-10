namespace Yup.Soporte.Api.Dtos
{
    public class CargaRequest
    {
        public int? TipoGestion { get; set; }
        public int IdEntidad { get; set; }
        public string CodigoEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string DatosAdicionales { get; set; }
        public IFormFile File { get; set; }
    }
}
