using Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Domain.AggregatesModel.Bloques;

namespace Yup.BulkProcess;

public interface IFilaArchivoCargaConverter<TFila, TFilaModel> where TFila : FilaArchivoCarga

{
    TFila ReadModel(TFilaModel model);
    TFilaModel CovertToModel(TFila fila);
    TFila ReadFilaModel(TFilaModel model, TFila fila);
}
