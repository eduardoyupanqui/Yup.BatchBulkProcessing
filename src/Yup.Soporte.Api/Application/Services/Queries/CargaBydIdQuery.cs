using MediatR;
using Yup.BulkProcess.Contracts.Response;
using Yup.Core;
using System;
using System.Collections.Generic;

namespace Yup.Soporte.Api.Application.Services.Queries;

public class CargaBydIdQuery<TProcesoMasivoDto> : IRequest<CargaByIdResponse<TProcesoMasivoDto>>
    where TProcesoMasivoDto : ProcesoMasivoResponseBase
{
    public Guid IdArchivoCarga { get; set; }
    public bool? esValido { get; set; }
}

public class CargaByIdResponse<TProcesoMasivoDto> : GenericResult
    where TProcesoMasivoDto : ProcesoMasivoResponseBase
{

    public IEnumerable<TProcesoMasivoDto> elementos { get; set; }
}
