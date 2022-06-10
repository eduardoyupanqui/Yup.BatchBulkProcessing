using Yup.Enumerados;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.Factories;

public class IntegracionEventGeneratorFactory
{
    public delegate IGenericIntegrationEventGenerator Delegate(ID_TBL_FORMATOS_CARGA tipoCarga);
    private readonly Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator> _integracionEventGenerator;
    public IntegracionEventGeneratorFactory(Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator> integracionEventGenerator)
    {
        _integracionEventGenerator = integracionEventGenerator;
    }
    public IGenericIntegrationEventGenerator Create(ID_TBL_FORMATOS_CARGA tipoCarga)
    {
        return _integracionEventGenerator(tipoCarga);
    }
}