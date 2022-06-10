using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Yup.Soporte.Api.Application.Commands;
using Yup.Core;

/// <summary>
/// Comando utilizado para crear un nuevo registro en la tabla  [maestro.entidad]
/// </summary>
namespace Soporte.Api.Application.Commands;

public class EditarProcesoMasivoCommand : AuditoriaCommand, IRequest<GenericResult>
{
    public int IdProcesoMasivo { get; private set; }
    public int IdTblTipoEntidad { get; set; }
    public int IdEntidad { get; set; }
    public int IdMasivaArchivo { get; set; }
    public int IdTblTipoProcesoCarga { get; set; }
    public string Descripcion { get; set; }
    public Guid Guid { get; set; }
    public bool EsEliminado { get; set; }

    public EditarProcesoMasivoCommand() { }
    public EditarProcesoMasivoCommand(
        int idprocesomasivo,
        int idtbltipoentidad,
        int identidad,
        int idmasivoarchivo,
        int idtbltipoprocesocarga,
        string descripcion
        )
    {
        IdProcesoMasivo = idprocesomasivo;
        IdTblTipoEntidad = idtbltipoentidad;
        IdEntidad = identidad;
        IdTblTipoProcesoCarga = idtbltipoprocesocarga;
        IdMasivaArchivo = idmasivoarchivo;
        Descripcion = descripcion;
    }

    public class EditarProcesoMasivoCommandHandler : IRequestHandler<EditarProcesoMasivoCommand, GenericResult>
    {
        private readonly IMediator _mediator;

        public EditarProcesoMasivoCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<GenericResult> Handle(EditarProcesoMasivoCommand message, CancellationToken cancellationToken)
        {
            var rsp = new GenericResult();
            return rsp;
        }
    }
}

