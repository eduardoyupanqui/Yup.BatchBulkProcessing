using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.Factories;

public class CrearCargaArchivoExcelCommandValidatorFactory
{
    public delegate ICargaCommandValidator<CrearCargaArchivoExcelCommand> Delegate(ID_TBL_FORMATOS_CARGA tipoCarga);
    private readonly Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaArchivoExcelCommand>> _crearCargaArchivoExcelCommandValidatorFactory;
    public CrearCargaArchivoExcelCommandValidatorFactory(Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaArchivoExcelCommand>> crearCargaArchivoExcelCommandValidatorFactory)
    {
        _crearCargaArchivoExcelCommandValidatorFactory = crearCargaArchivoExcelCommandValidatorFactory;
    }
    public ICargaCommandValidator<CrearCargaArchivoExcelCommand> Create(ID_TBL_FORMATOS_CARGA tipoCarga) 
    {
        return _crearCargaArchivoExcelCommandValidatorFactory(tipoCarga);
    }
}