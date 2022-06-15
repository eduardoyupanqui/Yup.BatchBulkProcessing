using System;

namespace Yup.BulkProcess;

public class ProcesoMasivoException : Exception
{
    public ProcesoMasivoException()
    { }

    public ProcesoMasivoException(string message) : base(message)
    { }
    public ProcesoMasivoException(string message, Exception innerException) : base(message, innerException)
    { }
}
