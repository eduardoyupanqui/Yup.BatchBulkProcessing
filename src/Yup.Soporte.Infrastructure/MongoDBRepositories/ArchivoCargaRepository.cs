using MongoDB.Driver;
using MongoDbGenericRepository;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.SeedworkMongoDB;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Yup.Soporte.Infrastructure.MongoDBRepositories;

public class ArchivoCargaRepository : BaseMongoRepository, IArchivoCargaRepository
{
    public ArchivoCargaRepository(IMongoDatabase mongoDatabase)
        : base(mongoDatabase)
    {
    }

    public ArchivoCarga Add(ArchivoCarga archivoCarga)
    {
        archivoCarga.IdArchivo = GetIncrementIdArchivoCarga();
        base.AddOne(archivoCarga);
        return archivoCarga;
    }

    public bool DeleteLogic(ArchivoCarga archivoCarga)
    {
        var updEliminacionLogica = Builders<ArchivoCarga>.Update
           .Set(b => b.EsEliminado, true)
           .Set(b => b.FechaModificacion, DateTime.Now)
           .Set(b => b.IpModificacion, archivoCarga.IpModificacion)
           .Set(b => b.UsuarioModificacion, archivoCarga.UsuarioModificacion);

        return base.UpdateOne(archivoCarga, updEliminacionLogica);
    }

    public bool ExisteArchivoEnProcesoParaEntidad(int idEntidad, int idTblTipoCarga)
    {
        var lTipoCarga = new List<TipoCarga>();

        var aListaTipoCargaModuloA = new int[] { (int)TipoCarga.STUDENTS };

        if (aListaTipoCargaModuloA.Contains(idTblTipoCarga))
            lTipoCarga = CargaEnumsExtensions.ParseTipoCargaList(',', string.Join(",", aListaTipoCargaModuloA)).ToList();


        return base.GetCursor<ArchivoCarga>(x =>
                                            x.IdEntidad == idEntidad &&
                                            lTipoCarga.Contains(x.Tipo) &&
                                          !(x.Estado == EstadoCarga.FINALIZADO ||
                                            x.Estado == EstadoCarga.CANCELADO) &&
                                            x.EsEliminado == false
                                           )
                                      .Project(x => x.Id)
                                      .FirstOrDefault() != default(Guid);
    }

    public async Task<ArchivoCarga> FindByIdAsync(Guid id)
    {
        return await GetByIdAsync<ArchivoCarga>(id);
    }
    public async Task<ArchivoCarga> FindByIdArchivoAsync(int idArchivo)
    {
        return await GetOneAsync<ArchivoCarga>(x => x.IdArchivo == idArchivo); ;
    }

    private int GetIncrementIdArchivoCarga()
    {
        var nombreColeccion = "archivosCarga";
        var filter = Builders<CollectionCounter>.Filter.Where(x => x.Id == nombreColeccion);
        var update = Builders<CollectionCounter>.Update.Inc(x => x.Identity, 1);
        var options = new FindOneAndUpdateOptions<CollectionCounter>() { IsUpsert = true, };
        var task = base.GetAndUpdateOne<CollectionCounter, string>(filter, update, options);
        task.Wait();
        var counter = task.Result;
        if (counter == null) return 1;
        return counter.Identity + 1;
    }

    public ArchivoCarga Update(ArchivoCarga archivoCarga)
    {
        base.UpdateOne(archivoCarga);
        return archivoCarga;
    }
    public bool UpdateStatus(ArchivoCarga archivoCarga, EstadoCarga estado, bool actualizarFechaAsociada)
    {
        return DoUpdateStatus(archivoCarga, estado, actualizarFechaAsociada);
    }
    public bool UpdateStatus(ArchivoCarga archivoCarga, EstadoCarga estado)
    {
        return DoUpdateStatus(archivoCarga, estado, false);
    }
    private bool DoUpdateStatus(ArchivoCarga archivoCarga, EstadoCarga estado, bool actualizarFechaAsociada)
    {
        UpdateDefinition<ArchivoCarga> updStatus;
        if (actualizarFechaAsociada == false)
        {
            updStatus = Builders<ArchivoCarga>.Update
                                                    .Set(b => b.Estado, estado)
                                                    .Set(b => b.FechaModificacion, DateTime.Now)
                                                   ;
            return base.UpdateOne(archivoCarga, updStatus);
        }
        switch (estado)
        {
            case EstadoCarga.EN_REGISTRO:
                updStatus = Builders<ArchivoCarga>.Update.Set(b => b.Estado, estado)
                                                         .Set(b => b.FechaRegistroInicio, DateTime.Now)
                                                         .Set(b => b.FechaModificacion, DateTime.Now)
                                                         ;
                break;
            case EstadoCarga.FINALIZADO:
                updStatus = Builders<ArchivoCarga>.Update.Set(b => b.Estado, estado)
                                                         .Set(b => b.FechaRegistroFin, DateTime.Now)
                                                         .Set(b => b.FechaModificacion, DateTime.Now)
                                                         ;
                break;
            default:
                updStatus = Builders<ArchivoCarga>.Update
                                                   .Set(b => b.Estado, estado)
                                                   .Set(b => b.FechaModificacion, DateTime.Now)
                                                  ;
                break;
        }


        return base.UpdateOne(archivoCarga, updStatus);
    }

    //public async Task<bool> AddMessage(Guid idArchivo, ArchivoCargaLogMessageType tipo, string mensaje)
    //{
    //    var archivoCarga = await base.GetByIdAsync<ArchivoCarga>(idArchivo);
    //    if (archivoCarga == null) return false;
    //    bool archivoTieneMensajesRegistrados = archivoCarga.Mensajes != null && archivoCarga.Mensajes.Count > 0;

    //    var mensajeEnRegistro = new ArchivoCargaLogMessage()
    //    {
    //        Tipo = tipo,
    //        Mensaje = mensaje,
    //        Fecha = DateTime.Now
    //    };

    //    UpdateDefinition<ArchivoCarga> updStatus;
    //    if (archivoTieneMensajesRegistrados)
    //    {
    //        updStatus = Builders<ArchivoCarga>.Update
    //                                            .AddToSet(b => b.Mensajes, mensajeEnRegistro)
    //                                            .Set(b => b.FechaModificacion, DateTime.Now);
    //    }
    //    else
    //    {
    //        updStatus = Builders<ArchivoCarga>.Update
    //                                            .Set(b => b.Mensajes, new List<ArchivoCargaLogMessage>() { mensajeEnRegistro })
    //                                            .Set(b => b.FechaModificacion, DateTime.Now);
    //    }

    //                                                ;
    //    return await base.UpdateOneAsync(archivoCarga, updStatus);
    //}

}
