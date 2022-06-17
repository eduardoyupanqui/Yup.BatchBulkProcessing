using Yup.Student.BulkProcess.Dtos;

namespace Yup.Student.BulkProcess.Application.Queries;

public class TransversalQueries : ITransversalQueries
{
    public Task<Dictionary<string, string>> ListarIdiomasExtranjeros()
    {
        return Task.FromResult(new Dictionary<string, string> 
        {
            { "1", "Español" },
            { "2", "Ingles" },
            { "3", "Frances" }
        });
    }

    public Task<Dictionary<string, string>> ListarLenguasNativas()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            { "1", "Ashaninca" },
            { "2", "Aimara" }
        });
    }

    public Task<EntidadResponseDto> ObtenerEntidadAsync(int idEntidad)
    {
        return Task.FromResult(new EntidadResponseDto { CodigoEntidad = "001", Nombre = "Entidad 1"});
    }
}
