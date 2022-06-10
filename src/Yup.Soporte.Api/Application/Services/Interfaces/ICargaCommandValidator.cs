using System;
using System.Threading.Tasks;
using Yup.Core;
using Yup.Soporte.Api.Application.Commands;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

/// <summary>
/// Interfaz base de CargaCommandValidator. Se diferencia solo por el tipo de crearCargaCommand que atiende
/// 
/// </summary>
/// <typeparam name="TCargaCommand"></typeparam>
public interface ICargaCommandValidator<TCargaCommand> 
    where TCargaCommand : CrearCargaCommand
{
    Task<GenericResult<Guid>> Validate(TCargaCommand command, bool completarDatosDescriptivos = false);
}

