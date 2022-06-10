using MongoDB.Driver;
using MongoDbGenericRepository;

namespace Yup.Soporte.Domain.AggregatesModel.Bloques;

public class BloqueCargaGenericRepository : BaseMongoRepository, IBloqueCargaGenericRepository
{
    public BloqueCargaGenericRepository(IMongoDatabase mongoDatabase)
         : base(mongoDatabase)
    {
    }
}
