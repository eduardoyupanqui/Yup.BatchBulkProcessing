using MediatR;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Yup.BulkProcess.Contracts.Request;
using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Dtos;
using Yup.Soporte.Api.Settings;

namespace Yup.Soporte.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CargaArchivoExcelController : ControllerBase
{
    private readonly ILogger<CargaArchivoExcelController> _logger;
    private readonly IMediator _mediator;
    private readonly CargaMasivaSettings _cargaMasivaSettings;

    public CargaArchivoExcelController(ILogger<CargaArchivoExcelController> logger, IMediator mediator, CargaMasivaSettings cargaMasivaSettings)
    {
        _logger = logger;
        _mediator = mediator;
        _cargaMasivaSettings = cargaMasivaSettings;
    }
    [Route("Adjuntar")]
    [HttpPost, DisableRequestSizeLimit]
    public async Task<IActionResult> Adjuntar([FromForm] CargaRequest request, CancellationToken cancellationToken = default)
    {
        string rutaBase = _cargaMasivaSettings.RutaBaseArchivos;
        DateTime fechaYhoraActual = DateTime.Now;
        var archivo = request.File;
        string extension = Path.GetExtension(archivo.FileName);
        string nombre = Path.GetFileNameWithoutExtension(archivo.FileName) + extension;
        string ruta = Path.Combine(fechaYhoraActual.ToString("yyyyMMddHHmmssfff"), nombre);
        long tamanio = archivo.Length;

        int idTblTipoCarga = 0;
        string nombrePlantilla = "";

        //Especificando Tipo de Carga STUDENTS
        idTblTipoCarga = (int)ID_TBL_FORMATOS_CARGA.STUDENTS;
        nombrePlantilla = "plantilla_students.xlsx";

        int cantidadRegistrosTotal = 0;
        bool esPlantilla = true;

        /*Contar cantidad  filas de la primera hoja*/
        ExcelPackage epArchivo = new ExcelPackage(archivo.OpenReadStream());
        if (epArchivo.Workbook.Worksheets.Count > 0)
        {
            ExcelWorksheet hojaExcel = epArchivo.Workbook.Worksheets[1];
            cantidadRegistrosTotal = hojaExcel.Dimension.End.Row - 1;
        }


        Directory.CreateDirectory(Path.Combine(rutaBase, fechaYhoraActual.ToString("yyyyMMddHHmmssfff")));
        using var fileStream = new FileStream(Path.Combine(rutaBase, ruta), FileMode.CreateNew);
        archivo.CopyTo(fileStream);
        fileStream.Close();

        CrearArchivoCommand command = new CrearArchivoCommand(
                    codigoEntidad: request.CodigoEntidad
                    , idEntidad: request.IdEntidad
                    , tipoGestion: request.TipoGestion
                    , nombreEntidad: request.NombreEntidad
                    , ruta: ruta
                    , nombre: nombre
                    , extension: extension
                    , tamanio: tamanio
                    , tieneFirmaDigital: null
                    , idTblTipoCarga: idTblTipoCarga
                    , cantidadRegistrosTotal: cantidadRegistrosTotal
                    , datosAdicionales: request.DatosAdicionales
                    , esPlantilla: esPlantilla
                    , estadoMatrizEntidadEstudiante: true
                );

        command.UsuarioRegistro = Guid.NewGuid();
        command.FechaRegistro = fechaYhoraActual;
        command.IpRegistro = "::";

        var commandResult = await _mediator.Send(command, cancellationToken);
        return Ok(commandResult);
    }
}
