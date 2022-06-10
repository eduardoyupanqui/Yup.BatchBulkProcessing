using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Domain.SeedworkMongoDB;
using Yup.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Yup.Soporte.Api.Application.Services;

public abstract class CargaRegistroBaseService<TBloqueCarga, TFilaArchivoCarga>
                                                                                           where TBloqueCarga : BloqueCarga<TFilaArchivoCarga>, new()
                                                                                           where TFilaArchivoCarga : FilaArchivoCarga
{
    public virtual int IdTblTipoCarga { get; protected set; }

    protected readonly IArchivoCargaRepository _archivoCargaRepository;
    protected readonly IBloqueCargaGenericRepository _bloquesGenericRepository;
    protected readonly IGenericCargaQueries _genericCargaQueries;

    private const int _1MB = 1024;
    private const int _tamañoPorDefectoDeBloque = 100;

    public CargaRegistroBaseService(
                                                          IArchivoCargaRepository archivoCargaRepository,
                                                          IBloqueCargaGenericRepository bloquesGenericRepository,
                                                          IGenericCargaQueries genericCargaQueries
                                                        )
    {
        _archivoCargaRepository = archivoCargaRepository ?? throw new ArgumentNullException(nameof(archivoCargaRepository));
        _bloquesGenericRepository = bloquesGenericRepository ?? throw new ArgumentNullException(nameof(bloquesGenericRepository));
        _genericCargaQueries = genericCargaQueries ?? throw new ArgumentNullException(nameof(genericCargaQueries));
    }

    protected async Task RegistrarBloques(Guid idArchivoCarga, IEnumerable<TFilaArchivoCarga> filas, string idUsuarioAutor, string ipOrigen)
    {
        var totalRegistros = filas.Count();
        var totalBloques = totalRegistros / _tamañoPorDefectoDeBloque;
        if (totalRegistros % _tamañoPorDefectoDeBloque > 0) totalBloques++;
        var registrosPorAsignar = totalRegistros;
        var lstBloquesAInsertar = new List<TBloqueCarga>();

        for (var i = 0; i < totalBloques; i++)
        {
            TBloqueCarga newBloque;
            if (registrosPorAsignar > _tamañoPorDefectoDeBloque)
            {
                registrosPorAsignar -= _tamañoPorDefectoDeBloque;

                //Registrar bloque
                newBloque = new TBloqueCarga()
                {
                    IdCarga = idArchivoCarga,
                    Estado = EstadoCarga.PENDIENTE
                };
                newBloque.FechaCreacion = DateTime.Now;
                newBloque.UsuarioCreacion = idUsuarioAutor;
                newBloque.IpCreacion = ipOrigen;
                newBloque.CantidadTotalElementos = _tamañoPorDefectoDeBloque;
                newBloque.FilaInicial = i * _tamañoPorDefectoDeBloque + 1;
                newBloque.FilaFinal = i * _tamañoPorDefectoDeBloque + _tamañoPorDefectoDeBloque;
                newBloque.Filas = filas.Skip(i * _tamañoPorDefectoDeBloque).Take(_tamañoPorDefectoDeBloque).ToList();
                lstBloquesAInsertar.Add(newBloque);
                continue;
            }

            newBloque = new TBloqueCarga()
            {
                IdCarga = idArchivoCarga,
                Estado = EstadoCarga.PENDIENTE
            };
            newBloque.FechaCreacion = DateTime.Now;
            newBloque.UsuarioCreacion = idUsuarioAutor;
            newBloque.IpCreacion = ipOrigen;
            newBloque.CantidadTotalElementos = registrosPorAsignar;
            newBloque.FilaInicial = i * _tamañoPorDefectoDeBloque + 1;
            newBloque.FilaFinal = i * _tamañoPorDefectoDeBloque + registrosPorAsignar;
            newBloque.Filas = filas.Skip(i * _tamañoPorDefectoDeBloque).Take(registrosPorAsignar).ToList();
            lstBloquesAInsertar.Add(newBloque);
        }

        await _bloquesGenericRepository.AddManyAsync(lstBloquesAInsertar);
    }

    protected async Task<GenericResult<Guid>> RegistrarCarga(CrearCargaCommand command, CrearCargaCommandArchivoData archivoData = null)
    {
        var result = new GenericResult<Guid>();

        #region Registro de archivo
        var objNuevoArchivoCarga = new ArchivoCarga()
        {
            IdEntidad = command.IdEntidad,
            CodigoEntidad = command.CodigoEntidad,
            Origen = (Domain.SeedworkMongoDB.OrigenCarga)command.IdOrigenCarga,
            Tipo = (Domain.SeedworkMongoDB.TipoCarga)command.IdTblTipoCarga,
            Estado = Domain.SeedworkMongoDB.EstadoCarga.PENDIENTE,
            CantidadTotalElementos = command.CantidadRegistrosTotal,

            Ruta = archivoData?.Ruta ?? "",
            Nombre = archivoData?.Nombre ?? $"{command.CodigoEntidad}_{Enum.GetName(typeof(TipoCargaServicioExterno), command.IdTblTipoCarga)}_{DateTime.Today.Year}.xlsx",
            Extension = archivoData?.Extension ?? "",
            Tamanio = archivoData?.Tamanio ?? 0L,
            TieneFirmaDigital = archivoData?.TieneFirmaDigital ?? false,

            UsuarioCreacion = command.UsuarioRegistro.ToString(),
            FechaCreacion = command.FechaRegistro,
            IpCreacion = command.IpRegistro

        };

        objNuevoArchivoCarga.Extended.UsuarioCreacionDescripcion = "Eduardo Yupanqui";
        objNuevoArchivoCarga.Extended.EntidadDescripcion = command.EntidadDescripcion;

        _archivoCargaRepository.Add(objNuevoArchivoCarga);
        var idArchivoCargaGenerado = objNuevoArchivoCarga.Id;

        #endregion

        var objArchivoCarga = await _archivoCargaRepository.FindByIdAsync(idArchivoCargaGenerado);

        #region Validacion de archivo
        if (objArchivoCarga == null)
        {
            return new GenericResult<Guid>(MessageType.Error, "No se pudo obtener datos del archivo. Por favor reinicie el proceso.");
        }
        if (objArchivoCarga.Estado != Domain.SeedworkMongoDB.EstadoCarga.PENDIENTE)
        {
            return new GenericResult<Guid>(MessageType.Error, "El archivo no se encuentra pendiente de atención.");
        }
        #endregion
        result.DataObject = idArchivoCargaGenerado;
        return result;

    }

}
