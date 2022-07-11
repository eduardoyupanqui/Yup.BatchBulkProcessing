using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yup.Student.BulkProcess.Application.Commands;
using Yup.Core;

namespace Yup.Student.BulkProcess.Controllers;

[ApiController]
[Route("[controller]")]
public class RunProcessController : ControllerBase
{

    private readonly ILogger<RunProcessController> _logger;
    private readonly IMediator _mediator;

    public RunProcessController(ILogger<RunProcessController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("CrearStudentBulk")]
    [ProducesResponseType(typeof(GenericResult<Guid>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CrearStudentBulk([FromQuery] Guid guidArchivo)
    {
        var command = new CrearStudentBulkCommand(guidArchivo);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("CrearStudentBlockBulk")]
    [ProducesResponseType(typeof(GenericResult<Guid>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CrearStudentBlockBulk([FromQuery] Guid guidArchivo, [FromQuery] Guid guidBloque)
    {
        var command = new CrearStudentBlockBulkCommand(guidArchivo, guidBloque);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
