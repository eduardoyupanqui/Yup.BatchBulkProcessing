using Yup.Soporte.Api.Settings;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Core;
using System;
using System.Threading.Tasks;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services;

public class CrearCargaServicioExternoBaseValidator : CrearCargaBaseValidator,
                                                          ICargaCommandValidator<CrearCargaServicioExternoCommand>
{
    public CrearCargaServicioExternoBaseValidator(CargaMasivaSettings cargaMasivaSettings,
         IArchivoCargaRepository archivoCargaRepository,
                                                                            IGenericCargaQueries genericCargaQueries
                                                                         )
        : base(cargaMasivaSettings, archivoCargaRepository, genericCargaQueries)
    {
    }

    public async Task<GenericResult<Guid>> Validate(CrearCargaServicioExternoCommand command, bool completarDatosDescriptivos = false)
    {
        var result = await ValidarGenerico(command, completarDatosDescriptivos);
        if (result.HasErrors) return result;

        //TODO: añadir otras validaciones específicas a la carga
        #region Validacion general de cantidad máxima de registros permitidos

        if (command.CantidadRegistrosTotal < 1 || command.CantidadRegistrosTotal > _cargaMasivaSettings.CantidadMaximaRegistros)
        { return new GenericResult<Guid>(MessageType.Error, $"El número de registros válidos debe estar entre 1 y {_cargaMasivaSettings.CantidadMaximaRegistros} Total: {command.CantidadRegistrosTotal}"); }

        #endregion
        return result;
    }
}
