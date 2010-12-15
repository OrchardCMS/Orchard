using System.Web;
using Autofac;

namespace Orchard.Environment {
    public class WorkContextModule : Module {
        protected override void Load(ContainerBuilder builder) {
            builder.RegisterType<WorkContextAccessor>()
                .As<IWorkContextAccessor>()
                .InstancePerMatchingLifetimeScope("shell");

            builder.Register(ctx => new WorkContextImplementation(ctx))
                .As<WorkContext>()
                .InstancePerMatchingLifetimeScope("work");

            builder.RegisterType<WorkContextProperty<HttpContextBase>>()
                .As<WorkContextProperty<HttpContextBase>>()
                .InstancePerMatchingLifetimeScope("work");

            builder.Register(ctx => ctx.Resolve<WorkContextProperty<HttpContextBase>>().Value)
                .As<HttpContextBase>()
                .InstancePerDependency();
        }
    }
}
