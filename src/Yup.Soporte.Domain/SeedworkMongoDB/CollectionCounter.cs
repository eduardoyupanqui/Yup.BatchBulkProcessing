using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace Yup.Soporte.Domain.SeedworkMongoDB;

[CollectionName("_counters")]
public class CollectionCounter : MongoDbGenericRepository.Models.IDocument<string>
{
    public CollectionCounter()
    {
        Version = 1;
    }
    public string Id { get; set; }
    [BsonElement("Idt")]
    public int Identity { get; set; }
    public int Version { get; set; }
}
