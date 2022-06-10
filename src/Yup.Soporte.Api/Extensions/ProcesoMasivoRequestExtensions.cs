using Yup.BulkProcess.Contracts.Request;
using System.Collections.Generic;

namespace Yup.Soporte.Api.Extensions;

public static class ProcesoMasivoRequestExtensions
{
    public static IEnumerable<TFilaOrigen> Foliar<TFilaOrigen>(this IEnumerable<TFilaOrigen> elementos) where TFilaOrigen : ProcesoMasivoRequestBase
    {
        var i = 1;
        foreach (var elem in elementos)
        {
            elem.NumeroElemento = i++;
        }
        return elementos;
    }

}
