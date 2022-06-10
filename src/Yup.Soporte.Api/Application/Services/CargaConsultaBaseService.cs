using MongoDB.Driver;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yup.Soporte.Api.Application.Services;

public abstract class CargaConsultaBaseService<TBloqueCarga, TFilaArchivoCarga>
                                                                                           where TBloqueCarga : BloqueCarga<TFilaArchivoCarga>, new()
                                                                                           where TFilaArchivoCarga : FilaArchivoCarga
{
    private IBloqueCargaGenericRepository _genericRepository;

    public CargaConsultaBaseService(IBloqueCargaGenericRepository genericRepository)
    {
        _genericRepository = genericRepository;
    }

    public IEnumerable<TFilaArchivoCarga> ObtenerFilasDeArchivoCarga(Guid idArchivoCarga, bool? esValido = null)
    {
        var lstFilasPorBloque = _genericRepository.GetCursor<TBloqueCarga>(x => x.IdCarga == idArchivoCarga &&
                                                                                    x.EsActivo == true &&
                                                                                    x.EsEliminado == false,
                                                                                    string.Empty)
                                                                    .Project(y => y.Filas)
                                                                    .ToList();
        if (esValido == null)
            return lstFilasPorBloque.SelectMany(x => x);

        return lstFilasPorBloque.SelectMany(x => x)
                                .Where(x => x.EsValido == esValido.Value);
    }

}
