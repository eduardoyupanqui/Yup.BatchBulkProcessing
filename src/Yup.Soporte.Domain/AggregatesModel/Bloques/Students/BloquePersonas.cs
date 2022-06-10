using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Domain.SeedworkMongoDB;

namespace Yup.Soporte.Domain.AggregatesModel.Bloques;

[CollectionName("bloquesPersonas")]
[BsonIgnoreExtraElements]
public class BloquePersonas : BloqueCarga<FilaArchivoPersona>
{
    public BloquePersonas()
    {
        Version = 1;
        Tipo = TipoCarga.STUDENTS;
    }
}
