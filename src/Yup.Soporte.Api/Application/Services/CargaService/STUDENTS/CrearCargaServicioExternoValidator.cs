using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Core;
using System;
using System.Threading.Tasks;
using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CrearCargaServicioExternoValidator : CrearCargaBaseValidator,
                                                          ICargaServicioExternoCommandValidator<DatosPersonaRequest>
{
    public CrearCargaServicioExternoValidator(CargaMasivaSettings cargaMasivaSettings,
         IArchivoCargaRepository archivoCargaRepository,
                                                                            IGenericCargaQueries genericCargaQueries
                                                                         )
        : base(cargaMasivaSettings, archivoCargaRepository, genericCargaQueries)
    {
    }

    public async Task<GenericResult<Guid>> Validate(CrearCargaServicioExternoCommand<DatosPersonaRequest> command, bool completarDatosDescriptivos = false)
    {
        var result = await ValidarGenerico(command, completarDatosDescriptivos);
        if (result.HasErrors) return result;

        ////TODO: añadir otras validaciones específicas a la carga "DATOS_DE_POSTULANTE"
        return result;
    }
}
