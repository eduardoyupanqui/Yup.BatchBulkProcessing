using MongoDB.Driver;
using Yup.Validation;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Yup.BulkProcess;

public class ConsultaBloqueService<TBloque, TFila, TFilaModel> : IConsultaBloqueService<TBloque, TFila, TFilaModel>
                                                  where TBloque : BloqueCarga<TFila>
                                                  where TFila : FilaArchivoCarga
                                                  where TFilaModel : IValidable
{
    private IBloqueCargaGenericRepository _bloqueGenericRepository;
    private IFilaArchivoCargaConverter<TFila, TFilaModel> _converter;

    public ConsultaBloqueService(IBloqueCargaGenericRepository bloqueGenericRepository,
                                  IFilaArchivoCargaConverter<TFila, TFilaModel> converter)
    {
        _bloqueGenericRepository = bloqueGenericRepository;
        _converter = converter;
    }

    /// <summary>
    /// Obtiene las ocurrencias de "UniqueKey" repetidas en todas las filas de los bloques de un archivoCarga representado como List<string>.
    /// </summary>
    /// <param name="idArchivoCarga">Identificador del archivoCarga cuyos bloques se van a consultar</param>
    /// <returns>Hashset con las claves únicas detectadas en el conjunto de datos</returns>
    public List<string> GetListUniqueKeysRepetidasDeIdArchivoCarga(Guid idArchivoCarga)
    {
        return GetHashUniqueKeysRepetidasDeIdArchivoCarga(idArchivoCarga).ToList();
    }
    /// <summary>
    /// Obtiene las ocurrencias de "UniqueKey" repetidas en todas las filas de los bloques de un archivoCarga representado como Hashset<string>.
    /// </summary>
    /// <param name="idArchivoCarga">Identificador del archivoCarga cuyos bloques se van a consultar</param>
    /// <returns>Hashset con las claves únicas detectadas en el conjunto de datos</returns>
    public HashSet<string> GetHashUniqueKeysRepetidasDeIdArchivoCarga(Guid idArchivoCarga)
    {
        HashSet<string> result = new HashSet<string>();
        List<string> lstUniqueKeys = new List<string>();

        var lstBloqueUniqueKeys = _bloqueGenericRepository.GetCursor<TBloque>(x => x.IdCarga == idArchivoCarga &&
                                                                                    x.EsActivo == true &&
                                                                                    x.EsEliminado == false)
                                                                    .Project(y => y.Filas
                                                                                    .Select(z => z.UniqueKey)
                                                                                    .Where(z => !string.IsNullOrWhiteSpace(z))
                                                                                    .ToList())
                                                                    .ToList();
        result = new HashSet<string>(lstBloqueUniqueKeys.SelectMany(x => x)
                                    .GroupBy(x => x)
                                    .Where(group => group.Count() > 1)
                                    .Select(group => group.Key)
                                    .Distinct());

        return result;
    }

    public List<string> GetListaUniqueKeysRepetidasDeIdArchivoCarga(Guid idArchivoCarga)
    {
        List<string> lstUniqueKeys = new List<string>();

        var lstBloqueUniqueKeys = _bloqueGenericRepository.GetCursor<TBloque>(x => x.IdCarga == idArchivoCarga &&
                                                                                    x.EsActivo == true &&
                                                                                    x.EsEliminado == false)
                                                                    .Project(y => y.Filas
                                                                                    .Select(z => z.UniqueKey)
                                                                                    .Where(z => !string.IsNullOrWhiteSpace(z))
                                                                                    .ToList())
                                                                    .ToList();

        lstUniqueKeys = lstBloqueUniqueKeys.SelectMany(x => x)
                                    .GroupBy(x => x)
                                    .Where(group => group.Count() > 1)
                                    .Select(group => group.Key)
                                    .ToList();

        return lstUniqueKeys;
    }

    public List<string> GetListaUniqueKeysDeIdArchivoCarga(Guid idArchivoCarga)
    {
        List<string> lstUniqueKeys = new List<string>();

        var lstBloqueUniqueKeys = _bloqueGenericRepository.GetCursor<TBloque>(x => x.IdCarga == idArchivoCarga &&
                                                                                    x.EsActivo == true &&
                                                                                    x.EsEliminado == false)
                                                                    .Project(y => y.Filas
                                                                                    .Select(z => z.UniqueKey)
                                                                                    .Where(z => !string.IsNullOrWhiteSpace(z))
                                                                                    .ToList())
                                                                    .ToList();

        lstUniqueKeys = lstBloqueUniqueKeys.SelectMany(x => x)
                                    .ToList();

        return lstUniqueKeys;
    }

    public IEnumerable<TFilaModel> ObtenerFilasDeArchivoModel(Guid idArchivoCarga, bool soloFilasValidas = false)
    {
        return ObtenerFilasDeArchivoCarga(idArchivoCarga, soloFilasValidas).Select(x => _converter.CovertToModel(x));            
    }
    public IEnumerable<TFilaModel> ObtenerFilasDeArchivoModelValidas(Guid idArchivoCarga)
    {
        return ObtenerFilasDeArchivoCarga(idArchivoCarga, soloFilasValidas: true).Select(x => _converter.CovertToModel(x));
    }

    public IEnumerable<TFila> ObtenerFilasDeArchivoCarga(Guid idArchivoCarga, bool soloFilasValidas = false)
    {
        var lstFilasPorBloque = _bloqueGenericRepository.GetCursor<TBloque>(x => x.IdCarga == idArchivoCarga &&
                                                                                    x.EsActivo == true &&
                                                                                    x.EsEliminado == false)
                                                                    .Project(y => y.Filas)
                                                                    .ToList();
        if (soloFilasValidas == false)
            return lstFilasPorBloque.SelectMany(x => x);

        return lstFilasPorBloque.SelectMany(x => x)
                                .Where(x => x.EsValido == true);
    }
    public IEnumerable<TFila> ObtenerFilasDeArchivoCargaValidas(Guid idArchivoCarga)
    {
        return ObtenerFilasDeArchivoCarga(idArchivoCarga, soloFilasValidas: true);
    }        
}
