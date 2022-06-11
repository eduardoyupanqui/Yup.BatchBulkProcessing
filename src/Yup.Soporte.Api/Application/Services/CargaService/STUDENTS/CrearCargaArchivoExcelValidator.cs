using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Core;
using System;
using System.Threading.Tasks;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CrearCargaArchivoExcelValidator : CrearCargaArchivoBaseValidator,
                                                         ICargaCommandValidator<CrearCargaArchivoExcelCommand>
{
    public CrearCargaArchivoExcelValidator(CargaMasivaSettings cargaMasivaSettings,
         IArchivoCargaRepository archivoCargaRepository,
                                                                            IGenericCargaQueries genericCargaQueries
                                                                         )
        : base(cargaMasivaSettings, archivoCargaRepository, genericCargaQueries)
    {
    }

    public async Task<GenericResult<Guid>> Validate(CrearCargaArchivoExcelCommand message, bool completarDatosDescriptivos = false)
    {
        var result = new GenericResult<Guid>();
        //TODO: Evaluar permisos de carga
        result = await ValidarArchivoGenerico(message, completarDatosDescriptivos);
        if (result.HasErrors) return result;

        result = ValidarNombreArchivo(message.CodigoEntidad,
                                                                 message.ArchivoNombre,
                                                                 "_PERSONAS_",
                                                                 DateTime.Now.Year);
        if (result.HasErrors) return result;
        //TODO: Añadir otras validaciones específicas del tipo de carga

        if (!message.FlagsPermisos.EstadoMatrizEntidadEstudiante)
        { return new GenericResult<Guid>(MessageType.Error, "No está permitido registrar ocurrencias."); }

        return result;
    }
}
