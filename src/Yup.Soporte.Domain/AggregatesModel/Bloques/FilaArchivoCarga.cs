using MongoDB.Bson.Serialization.Attributes;
using Yup.Validation;
using System.Collections.Generic;

namespace Yup.Soporte.Domain.AggregatesModel.Bloques;

public abstract class FilaArchivoCarga : IValidable
{
    public FilaArchivoCarga()
    {
        Observaciones = new List<string>();
    }
    [BsonElement("NroFil")]
    public int NumeroFila { get; set; }

    [BsonIgnore]
    public abstract string UniqueKey { get; }

    [BsonElement("Evl")]
    public bool Evaluado { get; set; }

    [BsonElement("Reg")]
    public bool Registrado { get; set; }

    [BsonElement("EsV")]
    public bool EsValido { get; set; }

    [BsonElement("Obs")]
    public List<string> Observaciones { get; set; }

}
