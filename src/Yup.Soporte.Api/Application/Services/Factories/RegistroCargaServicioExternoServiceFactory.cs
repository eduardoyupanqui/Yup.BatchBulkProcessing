using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Application.Services.Factories;

public class RegistroCargaServicioExternoServiceFactory
{
    public delegate ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand> Delegate(ID_TBL_FORMATOS_CARGA tipoCarga);
    private readonly Func<ID_TBL_FORMATOS_CARGA, ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand>> _registroCargaServicioExternoServiceFactory;
    public RegistroCargaServicioExternoServiceFactory(Func<ID_TBL_FORMATOS_CARGA, ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand>> registroCargaServicioExternoServiceFactory)
    {
        _registroCargaServicioExternoServiceFactory = registroCargaServicioExternoServiceFactory;
    }
    public ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand> Create(ID_TBL_FORMATOS_CARGA tipoCarga)
    {
        return _registroCargaServicioExternoServiceFactory(tipoCarga);
    }
}