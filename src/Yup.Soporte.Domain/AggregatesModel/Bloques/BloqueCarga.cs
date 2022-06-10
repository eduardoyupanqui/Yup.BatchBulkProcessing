using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using Yup.Soporte.Domain.SeedworkMongoDB;

namespace Yup.Soporte.Domain.AggregatesModel.Bloques;

public abstract class BloqueCarga<TFila> : ControlCargaMongoDocument
                                                  where TFila : FilaArchivoCarga
{
    [BsonElement("IdC", Order = 1)]
    public Guid IdCarga { get; set; }
    [BsonElement("IdCStr", Order = 2)]
    public string IdCargaString { get { return IdCarga.ToString(); } }

    [BsonElement("FIn")]
    public int FilaInicial { get; set; }
    [BsonElement("FFi")]
    public int FilaFinal { get; set; }
    [BsonElement("IdP")]
    public string IdProcesador { get; set; }
    [BsonElement("HnP")]
    public string HostnameProcesador { get; set; }
    [BsonElement("Fls")]
    public List<TFila> Filas { get; set; }
}
