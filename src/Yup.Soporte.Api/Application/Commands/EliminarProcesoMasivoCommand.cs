using System;
using System.Collections.Generic;
using MediatR;
using System.Runtime.Serialization;
using Yup.Core;

namespace Yup.Soporte.Api.Application.Commands;

public class EliminarProcesoMasivoCommand : AuditoriaCommand, IRequest<GenericResult>
{
    public int IdProcesoMasivo { get; private set; }

    public EliminarProcesoMasivoCommand(int idProcesoMasivo)
    {
        IdProcesoMasivo = idProcesoMasivo;

    }

    public class EliminarProcesoMasivoCommandHandler : IRequestHandler<EliminarProcesoMasivoCommand, GenericResult>
    {
        private readonly IMediator _mediator;

        public EliminarProcesoMasivoCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<GenericResult> Handle(EliminarProcesoMasivoCommand request, CancellationToken cancellationToken)
        {
            var response = new GenericResult();

            return response;
        }
    }
}
