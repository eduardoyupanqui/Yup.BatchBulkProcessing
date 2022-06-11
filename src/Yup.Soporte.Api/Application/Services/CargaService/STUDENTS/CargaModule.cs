using Autofac;
using Yup.Enumerados;
using Yup.Soporte.Api.Application.Commands;
using Yup.Soporte.Api.Infrastructure.AutofacModules;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.BulkProcess.Contracts.Request;
using Yup.Soporte.Api.Application.IntegrationEvents.Events;
using Yup.Soporte.Api.Application.Services.Interfaces;
using Yup.Soporte.Api.Application.Services.Factories;
using Yup.BulkProcess.Contracts.Response;

namespace Yup.Soporte.Api.Application.Services.CargaService.STUDENTS;

public class CargaModule : CargaSpecModule
{
    public CargaModule() : base(ID_TBL_FORMATOS_CARGA.STUDENTS)
    {
    }

    protected override void Load(ContainerBuilder builder)
    {
        #region Componentes para carga mediante archvo Excel
        builder.RegisterType<CrearCargaArchivoExcelValidator>()
        .As<ICargaCommandValidator<CrearCargaArchivoExcelCommand>>()
        .Keyed<ICargaCommandValidator<CrearCargaArchivoExcelCommand>>(_formatoCarga);

        builder.RegisterType<RegistroCargaArchivoExcelService>()
        .As<ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand, DatosPersonaRequest, BloquePersonas, FilaArchivoPersona>>()
        .Keyed<ICargaArchivoExcelRegistroService<CrearCargaArchivoExcelCommand>>(_formatoCarga);

        #endregion

        #region Componentes para carga mediante servicio externo
        builder.RegisterType<CrearCargaServicioExternoBaseValidator>()
        .As<ICargaCommandValidator<CrearCargaServicioExternoCommand>>()
        .Keyed<ICargaCommandValidator<CrearCargaServicioExternoCommand>>(_formatoCarga);

        builder.RegisterType<CargaServicioExternoRegistroService>()
        .As<ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand, DatosPersonaRequest, BloquePersonas, FilaArchivoPersona>>()
        .Keyed<ICargaServicioExternoRegistroService<CrearCargaServicioExternoCommand>>(_formatoCarga);

        #endregion

        builder.RegisterType<IntegrationEventGenerator>()
         .As<IIntegrationArchivoCargaService<ProcesarDatosPersonaMasivaIntegrationEvent>>()
         .Keyed<IGenericIntegrationEventGenerator>(_formatoCarga);



        builder.RegisterType<CargaConsultaService>()
            .As<ICargaConsultaService<BloquePersonas, FilaArchivoPersona, DatosPersonaResponse>>();
    }
}
