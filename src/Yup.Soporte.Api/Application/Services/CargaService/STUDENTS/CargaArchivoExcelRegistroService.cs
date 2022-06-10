using OfficeOpenXml;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Yup.BulkProcess.Contracts.Request;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class RegistroCargaArchivoExcelService : CargaArchivoExcelRegistroBaseService<BloquePersonas, FilaArchivoPersona>,
                                                                                        ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand, DatosPersonaRequest, BloquePersonas, FilaArchivoPersona>
{
    private readonly ILogger _logger;
    public RegistroCargaArchivoExcelService(
        IArchivoCargaRepository archivoCargaRepository,
        IBloqueCargaGenericRepository bloquesGenericRepository,
        IGenericCargaQueries genericCargaQueries,
        CargaMasivaSettings cargaMasivaSettings,
        ILogger<RegistroCargaArchivoExcelService> logger) : base(archivoCargaRepository, bloquesGenericRepository, genericCargaQueries, cargaMasivaSettings)
    {
        _logger = logger;
    }

    public FilaArchivoPersona ConvertirAFilaPersistencia(DatosPersonaRequest x, CrearCargaArchivoExcelCommand command)
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

    public IEnumerable<DatosPersonaRequest> LeerFilasOrigen(ExcelPackage excelPackage, CrearCargaArchivoExcelCommand command)
    {
        var lstFilasArchivoPostulante = new List<DatosPersonaRequest>();
        var libroRegistros = excelPackage.Workbook.Worksheets[0];
        for (var i = 2; i <= libroRegistros.Dimension.End.Row; i++)
        {
            lstFilasArchivoPostulante.Add(LeerFilaEstudiante(libroRegistros, i));
        }
        return lstFilasArchivoPostulante;
    }

    public async Task<GenericResult<Guid>> RegistrarCargaYBloques(CrearCargaArchivoExcelCommand command)
    {
        var result = new GenericResult<Guid>();

        #region Lectura de archivo excel
        var lecturaExcelResult = ObtenerArchivoOrigen(command);
        if (lecturaExcelResult.HasErrors)
        {
            result.AddError(lecturaExcelResult.Messages.FirstOrDefault().Message);
            return result;
        }
        #endregion

        try
        {
            result = await RegistrarArchivoCarga(command);
            if (result.HasErrors) { return result; }
            var idArchivoCarga = result.DataObject;
            await RegistrarBloques(idArchivoCarga,
                                                  LeerFilasOrigen(lecturaExcelResult.DataObject, command)
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
    private DatosPersonaRequest LeerFilaEstudiante(ExcelWorksheet excelWorksheet, int i)
    {
        var datosPersona = new DatosPersonaRequest();

        datosPersona.NumeroElemento = i - (_cargaMasivaSettings.FilaInicialLecturaExcel - 1);

        datosPersona.TipoDocumento = (excelWorksheet.Cells[i, 1].Text ?? "").Trim();
        datosPersona.NroDocumento = (excelWorksheet.Cells[i, 2].Text ?? "").Trim();
        datosPersona.LenguaNativa = (excelWorksheet.Cells[i, 3].Text ?? "").Replace(" ", string.Empty);
        datosPersona.IdiomaExtranjero = (excelWorksheet.Cells[i, 4].Text ?? "").Replace(" ", string.Empty);
        datosPersona.CondicionDiscapacidad = (excelWorksheet.Cells[i, 5].Text ?? "").Trim();
        datosPersona.CodigoORCID = (excelWorksheet.Cells[i, 6].Text ?? "").Trim();
        datosPersona.UbigeoDomicilio = (excelWorksheet.Cells[i, 7].Text ?? "").Trim();

        return datosPersona;
    }

}

