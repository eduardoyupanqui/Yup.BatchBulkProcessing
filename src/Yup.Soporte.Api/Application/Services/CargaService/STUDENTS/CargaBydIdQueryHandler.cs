using MediatR;
using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Soporte.Api.Application.Services.Queries;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System.Threading;
using System.Threading.Tasks;
using Yup.BulkProcess.Contracts.Response;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CargaBydIdQueryHandler : IRequestHandler<CargaBydIdQuery<DatosPersonaResponse>, CargaByIdResponse<DatosPersonaResponse>>
{
    private readonly ICargaConsultaService<BloquePersonas, FilaArchivoPersona, DatosPersonaResponse> _cargaConsultaService;

    public CargaBydIdQueryHandler(ICargaConsultaService<BloquePersonas, FilaArchivoPersona, DatosPersonaResponse> cargaConsultaService)
    {
        _cargaConsultaService = cargaConsultaService;
    }
    public async Task<CargaByIdResponse<DatosPersonaResponse>> Handle(CargaBydIdQuery<DatosPersonaResponse> request, CancellationToken cancellationToken)
    {
        var response = new CargaByIdResponse<DatosPersonaResponse>
        {
            elementos = _cargaConsultaService.ObtenerFilas(request.IdArchivoCarga, request.esValido)
        };
        return response;
    }
}
