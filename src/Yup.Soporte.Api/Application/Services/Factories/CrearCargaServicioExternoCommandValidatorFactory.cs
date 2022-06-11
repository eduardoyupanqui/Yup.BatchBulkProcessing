using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.Factories;

public class CrearCargaServicioExternoCommandValidatorFactory
{
    public delegate ICargaCommandValidator<CrearCargaServicioExternoCommand> Delegate(ID_TBL_FORMATOS_CARGA tipoCarga);
    private readonly Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaServicioExternoCommand>> _crearCargaArchivoExcelCommandValidatorFactory;
    public CrearCargaServicioExternoCommandValidatorFactory(Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaServicioExternoCommand>> crearCargaArchivoExcelCommandValidatorFactory)
    {
        _crearCargaArchivoExcelCommandValidatorFactory = crearCargaArchivoExcelCommandValidatorFactory;
    }
    public ICargaCommandValidator<CrearCargaServicioExternoCommand> Create(ID_TBL_FORMATOS_CARGA tipoCarga) 
    {
        return _crearCargaArchivoExcelCommandValidatorFactory(tipoCarga);
    }
}