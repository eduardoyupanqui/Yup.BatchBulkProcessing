using Yup.BulkProcess.Contracts.Request;
using System.Collections.Generic;
using System.Linq;
using Yup.Soporte.Domain.SeedworkMongoDB;

namespace Yup.Soporte.Api.Application.Commands;

public class CrearCargaServicioExternoCommand<TProcesoMasivoDto> : CrearCargaCommand
    where TProcesoMasivoDto : ProcesoMasivoRequestBase
{
    public CrearCargaServicioExternoCommand() : base()
    {
        IdOrigenCarga = (int)OrigenCarga.SERVICIO_EXTERNO;
    }
    public IEnumerable<TProcesoMasivoDto> Elementos { get; set; }

    public override int CantidadRegistrosTotal {
        get { if (Elementos == null) return 0; else return Elementos.Count(); }
        set { base.CantidadRegistrosTotal = value; }
    }
}
