using Yup.BulkProcess.Contracts.Request;
using System.Collections.Generic;
using System.Linq;

namespace Yup.Soporte.Api.Application.Commands;

public class CrearCargaServicioExternoCommand<TProcesoMasivoDto> : CrearCargaCommand
    where TProcesoMasivoDto : ProcesoMasivoRequestBase
{
    public CrearCargaServicioExternoCommand() : base()
    {
        IdOrigenCarga = 1; //Servicio Externo
    }
    public IEnumerable<TProcesoMasivoDto> elementos { get; set; }

    public override int CantidadRegistrosTotal {
        get { if (elementos == null) return 0; else return elementos.Count(); }
        set { base.CantidadRegistrosTotal = value; }
    }
}
