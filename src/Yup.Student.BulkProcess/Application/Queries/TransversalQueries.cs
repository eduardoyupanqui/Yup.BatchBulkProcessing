using Yup.Student.BulkProcess.Dtos;

namespace Yup.Student.BulkProcess.Application.Queries;

public class TransversalQueries : ITransversalQueries
{
    public Task<Dictionary<string, string>> ListarIdiomasExtranjeros()
    {
        return Task.FromResult(new Dictionary<string, string> 
        {
            { "30", "Español" },
            { "46", "Ingles" },
            { "35", "Frances" }
        });
    }

    public Task<Dictionary<string, string>> ListarLenguasNativas()
    {
        return Task.FromResult(new Dictionary<string, string>
        {
            { "2", "Aimara" },
            { "5", "Ashaninka" }
        });
    }

    public Task<EntidadResponseDto> ObtenerEntidadAsync(int idEntidad)
    {
        return Task.FromResult(new EntidadResponseDto { CodigoEntidad = "001", Nombre = "Entidad 1"});
    }
}
