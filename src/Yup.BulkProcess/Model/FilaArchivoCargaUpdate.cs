using System.Collections.Generic;

namespace Yup.BulkProcess;

public class FilaArchivoCargaUpdate
{
    public FilaArchivoCargaUpdate(int numeroFila)
    {
        #region Validaciones
        if (numeroFila < 1)
            throw new ArgumentException("Se requiere un número de fila mayor a cero", nameof(numeroFila));
        #endregion

        NumeroFila = numeroFila;
        UniqueKey = null;
        TipoUpdate = FilaArchivoCargaUpdateType.PorNumeroFila;
    }
    public FilaArchivoCargaUpdate(string uniqueKey)
    {
        #region Validaciones
        if (string.IsNullOrWhiteSpace(uniqueKey))
            throw new ArgumentException("Se requiere una llave única válida", nameof(uniqueKey));
        #endregion

        NumeroFila = 0;
        UniqueKey = uniqueKey;
        TipoUpdate = FilaArchivoCargaUpdateType.PorUniqueKey;
    }

    public int NumeroFila { get; private set; }
    public string UniqueKey { get; private set; }
    public FilaArchivoCargaUpdateType TipoUpdate { get; private set; }

    public bool? Registrado { get; set; }
    public bool? EsValido { get; set; }
    public List<string> Observaciones { get; set; }
}

public enum FilaArchivoCargaUpdateType
{
    PorNumeroFila,
    PorUniqueKey
}
