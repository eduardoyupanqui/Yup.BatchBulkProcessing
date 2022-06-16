using Yup.Validation;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using System;
using System.Collections.Generic;

namespace Yup.BulkProcess;

public interface IConsultaBloqueService<TBloque, TFila, TFilaModel>
                                                  where TBloque : BloqueCarga<TFila>
                                                  where TFila : FilaArchivoCarga
                                                  where TFilaModel : IValidable
{
    HashSet<string> GetHashUniqueKeysRepetidasDeIdArchivoCarga(Guid idArchivoCarga);
    List<string> GetListUniqueKeysRepetidasDeIdArchivoCarga(Guid idArchivoCarga);
    List<string> GetListaUniqueKeysDeIdArchivoCarga(Guid idArchivoCarga);

    IEnumerable<TFilaModel> ObtenerFilasDeArchivoModel(Guid idArchivoCarga, bool soloFilasValidas = false);
    IEnumerable<TFilaModel> ObtenerFilasDeArchivoModelValidas(Guid idArchivoCarga);

    IEnumerable<TFila> ObtenerFilasDeArchivoCarga(Guid idArchivoCarga, bool soloFilasValidas = false);
    IEnumerable<TFila> ObtenerFilasDeArchivoCargaValidas(Guid idArchivoCarga);       
}
