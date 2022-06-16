using System.Reflection;
using Autofac;
using MediatR;
using Yup.BulkProcess;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Infrastructure.MongoDBRepositories;

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
        #endregion
    }
}
