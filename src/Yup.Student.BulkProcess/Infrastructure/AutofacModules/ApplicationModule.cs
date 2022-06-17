using System.Reflection;
using Autofac;
using MediatR;
using Yup.BulkProcess;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Infrastructure.MongoDBRepositories;
using Yup.Student.BulkProcess.Application.Conversions;
using Yup.Student.BulkProcess.Application.Queries;
using Yup.Student.BulkProcess.Application.Validations;
using Yup.Student.BulkProcess.Infrastructure.Services;

namespace Yup.Student.BulkProcess.Infrastructure.AutofacModules;

public class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {

        #region Repositorios MongoDB
        builder.RegisterType<ArchivoCargaRepository>()
           .As<IArchivoCargaRepository>()
           .InstancePerLifetimeScope();
        builder.RegisterType<BloqueCargaGenericRepository>()
           .As<IBloqueCargaGenericRepository>()
           .InstancePerLifetimeScope();
        #endregion

        #region Servicios
        builder.RegisterType<SeguimientoProcesoBloqueService>()
          .As<ISeguimientoProcesoBloqueService>()
          .InstancePerLifetimeScope();

        builder.RegisterType(typeof(FilaArchivoStudentConverter))
            .As(typeof(IFilaArchivoCargaConverter<FilaArchivoPersona, Yup.Student.Domain.AggregatesModel.StudentAggregate.Student>));

        builder.RegisterGeneric(typeof(ConsultaBloqueService<,,>))
            .As(typeof(IConsultaBloqueService<,,>))
            .InstancePerLifetimeScope();

        builder.RegisterGeneric(typeof(ProcesoBloqueService<,,>))
            .As(typeof(IProcesoBloqueService<,,>))
            .InstancePerLifetimeScope();

        builder.RegisterType<StudentValidationContextGenerator>()
            .AsSelf()
            .InstancePerLifetimeScope();

        builder.RegisterType<TransversalQueries>()
            .As<ITransversalQueries>();

        builder.RegisterType<Fakes.FakeEventBus>()
            .As<IEventBus>()
            .InstancePerLifetimeScope();

        #endregion
    }
}
