using System.Reflection;
using Autofac;
using MediatR;
using Yup.Soporte.Api.Application.Queries;
using Yup.Soporte.Api.Infrastructure.Services;
using Yup.Soporte.Domain.AggregatesModel.ArchivoCargaAggregate;
using Yup.Soporte.Domain.AggregatesModel.Bloques;
using Yup.Soporte.Infrastructure.MongoDBRepositories;

namespace Yup.Soporte.Api.Infrastructure.AutofacModules;

public class ApplicationModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {

        builder.Register(c => new GenericCargaQueries())
          .As<IGenericCargaQueries>();

        #region Repositorios MongoDB
        builder.RegisterType<ArchivoCargaRepository>()
           .As<IArchivoCargaRepository>()
           .InstancePerLifetimeScope();
        builder.RegisterType<BloqueCargaGenericRepository>()
           .As<IBloqueCargaGenericRepository>()
           .InstancePerLifetimeScope();
        #endregion

        #region Servicios
        builder.RegisterType<Fakes.FakeEventBus>()
            .As<IEventBus>()
            .InstancePerLifetimeScope();
        #endregion
    }
}
