using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Yup.Soporte.Domain.SeedworkMongoDB;

public abstract class BaseMongoDocument : MongoDbGenericRepository.Models.Document
{
    public BaseMongoDocument()
    {
        EsActivo = true;
    }

    [BsonElement("_idStr", Order = 1)]
    public string IdString { get { return Id.ToString(); } }

    [BsonElement("Act", Order = 2)]
    public bool EsActivo { get; set; }

    [BsonElement("Eli", Order = 3)]
    public bool EsEliminado { get; set; }
}

public abstract class AuditableMongoDocument : BaseMongoDocument
{
    [BsonElement("CrU")]
    public string UsuarioCreacion { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    [BsonElement("CrF")]
    public DateTime? FechaCreacion { get; set; }
    [BsonElement("CrI")]
    public string IpCreacion { get; set; }

    [BsonElement("MdU")]
    public string UsuarioModificacion { get; set; }

    [BsonElement("MdF")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? FechaModificacion { get; set; }

    [BsonElement("MdI")]
    public string IpModificacion { get; set; }
}

public abstract class ControlCargaMongoDocument : AuditableMongoDocument
{
    [BsonElement("Tip")]
    [BsonRepresentation(BsonType.Int32)]
    public TipoCarga Tipo { get; set; }

    [BsonElement("Est")]
    [BsonRepresentation(BsonType.Int32)]
    public EstadoCarga Estado { get; set; }

    [BsonElement("Orig")]
    [BsonRepresentation(BsonType.Int32)]
    public OrigenCarga Origen { get; set; }

    //Marcas tiempo
    [BsonElement("FEvaIni")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? FechaEvaluacionInicio { get; set; }

    [BsonElement("FEvaFin")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? FechaEvaluacionFin { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    [BsonElement("FRegIni")]
    public DateTime? FechaRegistroInicio { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    [BsonElement("FRegFin")]
    public DateTime? FechaRegistroFin { get; set; }

    //Contadores
    [BsonElement("CTot")]
    public int CantidadTotalElementos { get; set; }

    [BsonElement("CEva")]
    public int CantidadEvaluados { get; set; }

    [BsonElement("CEvaObs")]
    public int CantidadEvaluadosObservados { get; set; }

    [BsonElement("CEvaVal")]
    public int CantidadEvaluadosValidos { get; set; }

    [BsonElement("CRegVal")]
    public int CantidadRegistradosValidos { get; set; }

    [BsonElement("CRegFal")]
    public int CantidadRegistradosFallidos { get; set; }
}

