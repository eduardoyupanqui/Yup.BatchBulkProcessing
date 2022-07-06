using Yup.Soporte.Api.Extensions;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CargaServicioExternoRegistroService : CargaRegistroBaseService<BloquePersonas, FilaArchivoPersona>,
                                                                                  ICargaServicioExternoRegistroService<DatosPersonaRequest, BloquePersonas, FilaArchivoPersona>

{
    private readonly ILogger _logger;
    public CargaServicioExternoRegistroService(
                                                                              IArchivoCargaRepository archivoCargaRepository,
                                                                              IBloqueCargaGenericRepository bloquesGenericRepository,
                                                                              IGenericCargaQueries genericCargaQueries
,
                                                                              ILogger<CargaServicioExternoRegistroService> logger)
      : base(archivoCargaRepository, bloquesGenericRepository, genericCargaQueries)
    {
        _logger = logger;
    }

    public FilaArchivoPersona ConvertirAFilaPersistencia(DatosPersonaRequest x, CrearCargaServicioExternoCommand<DatosPersonaRequest> command)
    {
        return new FilaArchivoPersona()
        {
            NumeroFila = x.NumeroElemento,

            codigo_orcid = x.CodigoORCID,
            condicion_discapacidad = x.CondicionDiscapacidad,
            idioma_extranjero = x.IdiomaExtranjero,
            lengua_nativa = x.LenguaNativa,
            nro_documento = x.NroDocumento,
            tipo_documento = x.TipoDocumento,
            ubigeo_residencia = x.UbigeoDomicilio
        };
    }

    public async Task<GenericResult<(Guid, IEnumerable<Guid>)>> RegistrarCargaYBloques(CrearCargaServicioExternoCommand<DatosPersonaRequest> command)
    {
        var result = new GenericResult<(Guid, IEnumerable<Guid>)>();

        try
        {
            var registrarResult = await RegistrarCarga(command);
            if (registrarResult.HasErrors)
            {
                result.AddError(registrarResult.Messages.FirstOrDefault().Message);
                return result;
            }
            var idArchivoCarga = registrarResult.DataObject;
            var bloques = await RegistrarBloques(idArchivoCarga,
                                                  command.Elementos.Foliar()
                                                  .Select(x => ConvertirAFilaPersistencia(x, command)).ToList(),
                                                  command.UsuarioRegistro.ToString(),
                                                  command.IpRegistro
                                                  );
            result.DataObject = (idArchivoCarga, bloques);
            return result;
        }
        catch (Exception ex)
        {
            var errorMsg = "Ocurrió un error en el servicio de registro del archivo de carga masiva.";
            result.AddError(errorMsg);
            _logger.LogError(ex, $"{errorMsg} {ex.Message}");
        }
        return result;
    }
}
