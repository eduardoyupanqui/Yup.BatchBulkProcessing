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
                                                                                  ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand, DatosPersonaRequest, BloquePersonas, FilaArchivoPersona>

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

    public FilaArchivoPersona ConvertirAFilaPersistencia(DatosPersonaRequest x, CrearCargaServicioExternoCommand command)
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

    public IEnumerable<DatosPersonaRequest> LeerFilasOrigen(CrearCargaServicioExternoCommand command)
    {
        var lstFilasArchivoPostulante = new List<DatosPersonaRequest>();
        var libroRegistros = command.Elementos;
        foreach (var registro in libroRegistros)
        {
            lstFilasArchivoPostulante.Add(LeerFilaEstudiante(registro));
        }
        return lstFilasArchivoPostulante;
    }

    public async Task<GenericResult<Guid>> RegistrarCargaYBloques(CrearCargaServicioExternoCommand command)
    {
        var result = new GenericResult<Guid>();

        try
        {
            result = await RegistrarCarga(command);
            if (result.HasErrors) { return result; }
            var idArchivoCarga = result.DataObject;
            await RegistrarBloques(idArchivoCarga,
                                                  LeerFilasOrigen(command).Foliar()
                                                  .Select(x => ConvertirAFilaPersistencia(x, command)).ToList(),
                                                  command.UsuarioRegistro.ToString(),
                                                  command.IpRegistro
                                                  );
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

    private DatosPersonaRequest LeerFilaEstudiante(Dictionary<string,string> dictionaryObject)
    {
        var datosPersona = new DatosPersonaRequest();

        datosPersona.TipoDocumento = (dictionaryObject["tipoDocumento"] ?? "").Trim();
        datosPersona.NroDocumento = (dictionaryObject["nroDocumento"] ?? "").Trim();
        datosPersona.LenguaNativa = (dictionaryObject["lenguaNativa"] ?? "").Replace(" ", string.Empty);
        datosPersona.IdiomaExtranjero = (dictionaryObject["idiomaExtranjero"] ?? "").Replace(" ", string.Empty);
        datosPersona.CondicionDiscapacidad = (dictionaryObject["condicionDiscapacidad"] ?? "").Trim();
        datosPersona.CodigoORCID = (dictionaryObject["codigoORCID"] ?? "").Trim();
        datosPersona.UbigeoDomicilio = (dictionaryObject["ubigeoDomicilio"] ?? "").Trim();

        return datosPersona;
    }
}
