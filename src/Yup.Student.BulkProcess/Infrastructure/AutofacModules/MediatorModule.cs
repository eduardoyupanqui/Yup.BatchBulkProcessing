using System.Reflection;
using Autofac;
using MediatR;

namespace Yup.Student.BulkProcess.Infrastructure.AutofacModules;

public class MediatorModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();
        // Register all the Command classes(they implement IRequestHandler) in assembly holding the Commands
        builder.RegisterAssemblyTypes(typeof(Program).GetTypeInfo().Assembly)
            .AsClosedTypesOf(typeof(IRequestHandler<,>));



        builder.Register<ServiceFactory>(context =>
        {
            var componentContext = context.Resolve<IComponentContext>();
            return t => { object o; return componentContext.TryResolve(t, out o) ? o : null; };
        });
    }
}
