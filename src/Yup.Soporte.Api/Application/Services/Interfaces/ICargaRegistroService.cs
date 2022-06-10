using Yup.Core;
using System;
using System.Threading.Tasks;
using Yup.Soporte.Api.Application.Commands;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

/// <summary>
/// Interfaz base de RegistroCargaService. Se diferencia solo por el tipo de crearCargaCommand que atiende
/// 
/// </summary>
/// <typeparam name="TCargaCommand"></typeparam>
public interface ICargaRegistroService<TCargaCommand> where TCargaCommand : CrearCargaCommand
{
    Task<GenericResult<Guid>> RegistrarCargaYBloques(TCargaCommand command);
}
