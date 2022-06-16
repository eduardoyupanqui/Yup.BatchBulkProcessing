using System.Net;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Yup.BulkProcess.Application.Commands;
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

    [HttpGet(Name = "CrearStudentBulk")]
    [ProducesResponseType(typeof(GenericResult<Guid>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> CrearStudentBulk([FromQuery] CrearStudentBulkCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
