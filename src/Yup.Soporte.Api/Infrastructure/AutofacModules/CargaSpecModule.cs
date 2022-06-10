using Yup.Enumerados;

namespace Yup.Soporte.Api.Infrastructure.AutofacModules;

public class CargaSpecModule : Autofac.Module
{
    protected ID_TBL_FORMATOS_CARGA _formatoCarga;
    public CargaSpecModule(ID_TBL_FORMATOS_CARGA formatoCarga)
    {
        _formatoCarga = formatoCarga;
    }
}
