using Yup.Student.BulkProcess.Dtos;

namespace Yup.Student.BulkProcess.Application.Queries;

public interface ITransversalQueries
{
    Task<Dictionary<string, string>> ListarLenguasNativas();
    Task<Dictionary<string, string>> ListarIdiomasExtranjeros();
    Task<EntidadResponseDto> ObtenerEntidadAsync(int idEntidad);
}
