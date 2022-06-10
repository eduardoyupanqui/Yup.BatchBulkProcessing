using Yup.Soporte.Domain.AggregatesModel.Bloques;

namespace Soporte.Domain.AggregatesModel.Bloques;

public interface IFilaArchivoValidator<T> where T : FilaArchivoCarga
{
    void Validate(T fila);
}
