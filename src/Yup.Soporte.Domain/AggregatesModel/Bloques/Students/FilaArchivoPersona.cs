using MongoDB.Bson.Serialization.Attributes;

namespace Yup.Soporte.Domain.AggregatesModel.Bloques;

public class FilaArchivoPersona : FilaArchivoCarga
{
    [BsonElement("TipDoc")]
    public string tipo_documento { get; set; }

    [BsonElement("NroDoc")]
    public string nro_documento { get; set; }

    [BsonElement("LenNat")]
    public string lengua_nativa { get; set; }

    [BsonElement("IdiExt")]
    public string idioma_extranjero { get; set; }

    [BsonElement("CndDis")]
    public string condicion_discapacidad { get; set; }

    [BsonElement("CodORC")]
    public string codigo_orcid { get; set; }

    [BsonElement("UbiRes")]
    public string ubigeo_residencia { get; set; }

    [BsonIgnoreIfNull]
    public FilaArchivoPersonaExtended Extended { get; set; }

    [BsonIgnoreIfNull]
    public override string UniqueKey
    {
        get
        {
            if (string.IsNullOrWhiteSpace(tipo_documento)
                || string.IsNullOrWhiteSpace(nro_documento)
                )
            {
                return null;
            }

            return $"{tipo_documento}:{nro_documento}";
        }
    }
}
public class FilaArchivoPersonaExtended
{
    [BsonElement("IdPer")]
    public int id_persona { get; set; }
    [BsonElement("IdUbiRes")]
    public int id_ubigeo_residencia { get; set; }
}
