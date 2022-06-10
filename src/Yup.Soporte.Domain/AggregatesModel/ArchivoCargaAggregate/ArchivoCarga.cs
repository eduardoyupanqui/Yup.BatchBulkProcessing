using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using Yup.Soporte.Domain.SeedworkMongoDB;
using System;
using System.Collections.Generic;

namespace Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;

[CollectionName("archivosCarga")]
[BsonIgnoreExtraElements]
public class ArchivoCarga : ControlCargaMongoDocument
{
    public ArchivoCarga()
    {
        Version = 1;
        Extended = new ArchivoCargaExtended();
        //Mensajes = new List<ArchivoCargaLogMessage>();
    }

    /// <summary>
    /// Id ficticio para facilitar búsquedas desde el el cliente. No confundir con el campo "Id" (Guid)
    /// </summary>
    [BsonElement("IdArch")]
    public int IdArchivo { get; set; }

    [BsonElement("IdEnt")]
    public int IdEntidad { get; set; }

    [BsonElement("CodEnt")]
    public string CodigoEntidad { get; set; }

    //Archivo físico
    [BsonElement("Rut")]
    public string Ruta { get; set; }

    [BsonElement("Nom")]
    public string Nombre { get; set; }

    [BsonElement("Ext")]
    public string Extension { get; set; }

    [BsonElement("Tam")]
    public long Tamanio { get; set; }

    [BsonElement("Fir")]
    public bool? TieneFirmaDigital { get; set; }

    [BsonIgnore]
    public bool EstaPendienteDeProcesamiento {
        get {
            if (this.Estado == EstadoCarga.FINALIZADO || this.Estado == EstadoCarga.CANCELADO)
            {
                return false;
            }
            return true;
        }
    }
    public ArchivoCargaExtended Extended { get; set; }

    //public List<ArchivoCargaLogMessage> Mensajes { get; set; }
}

public class ArchivoCargaExtended
{
    public string EntidadDescripcion { get; set; }
    public string TipoDescripcion { get; set; }
    public string EstadoDescripcion { get; set; }
    public string UsuarioCreacionDescripcion { get; set; }

}

public class ArchivoCargaLogMessage
{
    public ArchivoCargaLogMessageType Tipo { get; set; }
    public DateTime Fecha { get; set; }
    public string Mensaje { get; set; }

}
public enum ArchivoCargaLogMessageType
{
    Info = 0,
    Warning = 1,
    Error = 2
}
