using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Domain.AggregatesModel.Bloques;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

/// <summary>
/// Interfaz genérica de RegistroCargaServicioExternoService para comandos de tipo "CrearCargaServicioExternoCommand"
/// </summary>
/// <typeparam name="TCargaCommand"></typeparam>
public interface ICargaServicioExternoRegistroService<TCargaCommand> : ICargaRegistroService<TCargaCommand>
    where TCargaCommand : CrearCargaServicioExternoCommand
{

}

/// <summary>
/// Interfaz especializada de RegistroCargaServicioExternoService. Se declara para cada tipo de carga (ej: postulantes, ingresantes)
/// </summary>
/// <typeparam name="TCargaCommand"></typeparam>
/// <typeparam name="TFilaOrigen"></typeparam>
/// <typeparam name="TBloqueCarga"></typeparam>
/// <typeparam name="TFilaCarga"></typeparam>
public interface ICargaServicioExternoRegistroService<TCargaCommand, TFilaOrigen, TBloqueCarga, TFilaCarga> : ICargaServicioExternoRegistroService<TCargaCommand>
                                                                                                             where TCargaCommand : CrearCargaServicioExternoCommand
                                                                                                             where TFilaOrigen : ProcesoMasivoRequestBase, new()
                                                                                                             where TBloqueCarga : BloqueCarga<TFilaCarga>, new()
                                                                                                             where TFilaCarga : FilaArchivoCarga
{
    TFilaCarga ConvertirAFilaPersistencia(TFilaOrigen fila, CrearCargaServicioExternoCommand command);
    IEnumerable<TFilaOrigen> LeerFilasOrigen(TCargaCommand command);
}
