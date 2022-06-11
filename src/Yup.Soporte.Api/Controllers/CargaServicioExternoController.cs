using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yup.BulkProcess.Contracts.Request;
using Yup.BulkProcess.Contracts.Response;
using Yup.Core;
using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Queries;

namespace Yup.Soporte.Api.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class CargaServicioExternoController : ControllerBase
{
    private readonly IMediator _mediator;

    public CargaServicioExternoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("Students")]
    [ProducesResponseType(typeof(GenericResult<Guid>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> Students([FromBody] CrearCargaServicioExternoCommand command, CancellationToken cancellationToken)
    {
        var result = new GenericResult<Guid>();
        #region Lectura de usuario y entidad
        command.IdTblTipoCarga = ID_TBL_FORMATOS_CARGA.STUDENTS;

        command.FechaRegistro = DateTime.Now;
        command.UsuarioRegistro = Guid.NewGuid();
        command.UsuarioCreacionDescripcion = "Eduardo";
        command.IpRegistro = "::";
        #endregion
        result = await _mediator.Send(command, cancellationToken);
        return result.HasErrors ? BadRequest(result) : (IActionResult)Ok(result);
    }

    [HttpGet("Students")]
    [ProducesResponseType(typeof(CargaByIdResponse<DatosPersonaResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetStudents([FromQuery] CargaBydIdQuery<DatosPersonaResponse> request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(request, cancellationToken);
        return result.HasErrors ? BadRequest(result) : (IActionResult)Ok(result);
    }
}
