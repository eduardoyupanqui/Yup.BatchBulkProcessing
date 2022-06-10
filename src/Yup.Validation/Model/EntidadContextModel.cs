namespace Yup.Validation
{
    public class EntidadContextModel
    {
        public string CodigoEntidad { get; set; }
        public int IdTblTipoEntidad { get; set; }
        public int IdTblTipoGestion { get; set; }
        public int IdTblTipoAutorizacionEntidad { get; set; }
        public int IdTblEstadoVigencia { get; set; }
        public bool EstaEnProcesoCese { get; set; }
    }
}
