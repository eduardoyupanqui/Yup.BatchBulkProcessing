using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Api.Application.Commands;

namespace Yup.Soporte.Api.Application.Services.Interfaces;

public interface ICargaServicioExternoCommandValidator<TFilaOrigen> : ICargaCommandValidator<CrearCargaServicioExternoCommand<TFilaOrigen>>
                                                                                                                          where TFilaOrigen : ProcesoMasivoRequestBase, new()
{
}
