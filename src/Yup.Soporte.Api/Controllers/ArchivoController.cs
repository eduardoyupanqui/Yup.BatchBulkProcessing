using Microsoft.AspNetCore.Mvc;

namespace Yup.Soporte.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ArchivoController : ControllerBase
{
    private readonly ILogger<ArchivoController> _logger;

    public ArchivoController(ILogger<ArchivoController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "")]
    public IEnumerable<object> Get()
    {
        string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        return Enumerable.Range(1, 5).Select(index => new
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
