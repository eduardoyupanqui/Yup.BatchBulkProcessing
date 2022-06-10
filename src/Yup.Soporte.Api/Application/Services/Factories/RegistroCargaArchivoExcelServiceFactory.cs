using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.Factories;

public class RegistroCargaArchivoExcelServiceFactory
{
    public delegate ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand> Delegate(ID_TBL_FORMATOS_CARGA tipoCarga);
    private readonly Func<ID_TBL_FORMATOS_CARGA, ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>> _registroCargaArchivoExcelServiceFactory;
    public RegistroCargaArchivoExcelServiceFactory(Func<ID_TBL_FORMATOS_CARGA, ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>> registroCargaArchivoExcelServiceFactory)
    {
        _registroCargaArchivoExcelServiceFactory = registroCargaArchivoExcelServiceFactory;
    }
    public ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand> Create(ID_TBL_FORMATOS_CARGA tipoCarga)
    {
        return _registroCargaArchivoExcelServiceFactory(tipoCarga);
    }
}