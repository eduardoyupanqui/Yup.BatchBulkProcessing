using Yup.Soporte.Domain.SeedworkMongoDB;
using System;
using System.Threading.Tasks;

namespace Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;

public interface IArchivoCargaRepository
{
    ArchivoCarga Add(ArchivoCarga archivoCarga);
    ArchivoCarga Update(ArchivoCarga archivoCarga);
    Task<ArchivoCarga> FindByIdAsync(Guid id);
    Task<ArchivoCarga> FindByIdArchivoAsync(int idArchivo);
    bool DeleteLogic(ArchivoCarga archivoCarga);
    bool ExisteArchivoEnProcesoParaEntidad(int idEntidad, int idTblTipoCarga);

    bool UpdateStatus(ArchivoCarga archivoCarga, EstadoCarga estado);
    bool UpdateStatus(ArchivoCarga archivoCarga, EstadoCarga estado, bool actualizarFechaAsociada);

    //Task<bool> AddMessage(Guid idArchivo, ArchivoCargaLogMessageType tipo, string mensaje);
}
