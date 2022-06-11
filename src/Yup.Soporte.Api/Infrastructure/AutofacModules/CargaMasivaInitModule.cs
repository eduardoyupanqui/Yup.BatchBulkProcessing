using Autofac;
using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Application.Services.Factories;
using Yup.Soporte.Api.Application.Services.Interfaces;

namespace Yup.Soporte.Api.Infrastructure.AutofacModules;

public class CargaMasivaInitModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        #region Registro de resolver de IRegistroCargaArchivoExcelService
        builder.Register<Func<ID_TBL_FORMATOS_CARGA, ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>>>(c =>
        {
            var componentContext = c.Resolve<IComponentContext>();
            return (idTblFormatoCargaEnum) =>
            {
                var registroCargaArchivoExcelService = componentContext.ResolveKeyed<ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>>(idTblFormatoCargaEnum);
                return registroCargaArchivoExcelService;
            };
        });
        builder.RegisterType<RegistroCargaArchivoExcelServiceFactory>();
        #endregion

        #region Registro de resolver de generador de eventos de integración
        builder.Register<Func<ID_TBL_FORMATOS_CARGA, IGenericIntegrationEventGenerator>>(c =>
        {
            var componentContext = c.Resolve<IComponentContext>();
            return (idTblFormatoCargaEnum) =>
            {
                var integracionArchivoCargaService = componentContext.ResolveKeyed<IGenericIntegrationEventGenerator>(idTblFormatoCargaEnum);
                return integracionArchivoCargaService;
            };
        });
        builder.RegisterType<IntegracionEventGeneratorFactory>();
        #endregion

        #region Registro de resolver de validador especializado para archivos Excel
        builder.Register<Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaArchivoExcelCommand>>>(c =>
        {
            var componentContext = c.Resolve<IComponentContext>();
            return (idTblFormatoCargaEnum) =>
            {
                var validator = componentContext.ResolveKeyed<ICargaCommandValidator<CrearCargaArchivoExcelCommand>>(idTblFormatoCargaEnum);
                return validator;
            };
        });
        builder.RegisterType<CrearCargaArchivoExcelCommandValidatorFactory>();
        #endregion




        #region Registro de resolver de IRegistroCargaArchivoExcelService
        builder.Register<Func<ID_TBL_FORMATOS_CARGA, ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand>>>(c =>
        {
            var componentContext = c.Resolve<IComponentContext>();
            return (idTblFormatoCargaEnum) =>
            {
                var registroCargaServicioExternoService = componentContext.ResolveKeyed<ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand>>(idTblFormatoCargaEnum);
                return registroCargaServicioExternoService;
            };
        });
        builder.RegisterType<RegistroCargaServicioExternoServiceFactory>();
        #endregion


        #region Componentes para carga mediante servicio externo
        builder.Register<Func<ID_TBL_FORMATOS_CARGA, ICargaCommandValidator<CrearCargaServicioExternoCommand>>>(c =>
        {
            var componentContext = c.Resolve<IComponentContext>();
            return (idTblFormatoCargaEnum) =>
            {
                var validator = componentContext.ResolveKeyed<ICargaCommandValidator<CrearCargaServicioExternoCommand>>(idTblFormatoCargaEnum);
                return validator;
            };
        });
        builder.RegisterType<CrearCargaServicioExternoCommandValidatorFactory>();
        #endregion
    }
}
