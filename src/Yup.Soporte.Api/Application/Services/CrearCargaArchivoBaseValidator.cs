using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services;
using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Yup.Soporte.Api.Application.Services;

public abstract class CrearCargaArchivoBaseValidator : CrearCargaBaseValidator
{
    private const int _1MB = 1024;

    public CrearCargaArchivoBaseValidator(CargaMasivaSettings cargaMasivaSettings,
         IArchivoCargaRepository archivoCargaRepository,
                                                                            IGenericCargaQueries genericCargaQueries
                                                                         )
        : base(cargaMasivaSettings, archivoCargaRepository, genericCargaQueries)
    {
    }

    protected async Task<GenericResult<Guid>> ValidarArchivoGenerico(CrearCargaArchivoExcelCommand message, bool completarDatosDescriptivos = false)
    {
        var response = new GenericResult<Guid>();

        var result = await ValidarGenerico(message, completarDatosDescriptivos);
        if (result.HasErrors) return result;

        #region Validacion de configuracion de acceso a archivos
        if (string.IsNullOrWhiteSpace(_cargaMasivaSettings.RutaBaseArchivos))
        { return new GenericResult<Guid>(MessageType.Error, "Se detectó un problema con la configuración de la ruta base de archivos. Por favor notifíquelo al administrador del sistema."); }

        #endregion

        if (string.IsNullOrWhiteSpace(message.ArchivoExtension))
        { return new GenericResult<Guid>(MessageType.Error, "Solicitud no válida. Extensión de archivo requerida."); }

        if (_cargaMasivaSettings.ExtensionesArchivoPermitidas.Contains(message.ArchivoExtension, StringComparer.OrdinalIgnoreCase) == false)
        { return new GenericResult<Guid>(MessageType.Error, $"Solicitud no válida. Extensión de archivo ({message.ArchivoExtension}) no permitida"); }

        if (Math.Round(Convert.ToDouble(message.ArchivoTamanio / _1MB), 2) > _cargaMasivaSettings.TamanoMaximoDeArchivoEnMB * _1MB)
        { return new GenericResult<Guid>(MessageType.Error, $"Solicitud no válida.El tamaño del archivo excede el límite permitido ({_cargaMasivaSettings.TamanoMaximoDeArchivoEnMB} MB)"); }

        if (!message.ArchivoBasadoEnPlantilla)
        { return new GenericResult<Guid>(MessageType.Error, "Las columnas del archivo no corresponden a una plantilla válida"); }

        #region Validacion general de cantidad máxima de registros permitidos

        if (message.CantidadRegistrosTotal < 1 || message.CantidadRegistrosTotal > _cargaMasivaSettings.CantidadMaximaRegistros)
        { return new GenericResult<Guid>(MessageType.Error, $"El número de registros válidos debe estar entre 1 y {_cargaMasivaSettings.CantidadMaximaRegistros} Total: {message.CantidadRegistrosTotal}"); }

        #endregion

        return response;
    }

    protected GenericResult<Guid> ValidarNombreArchivo(string codigoEntidad, string archivoNombre, string patronNombreArchivo, int anioPermitido)
    {
        var response = new GenericResult<Guid>();

        var lstPatrones = new List<string>();
        foreach (var extensionPermitida in _cargaMasivaSettings.ExtensionesArchivoPermitidas)
        {
            lstPatrones.Add($"^{codigoEntidad}{patronNombreArchivo}{anioPermitido}" + "[A-Za-z0-9\\-_.()\\[\\]{}]*\\" + extensionPermitida + "$"); ;
        }
        if (lstPatrones.Any(x => new Regex(x).IsMatch(archivoNombre)) == false)
        {
            return new GenericResult<Guid>(MessageType.Error, "El nombre del archivo no corresponde a un formato válido");
        }
        return response;
    }

}
