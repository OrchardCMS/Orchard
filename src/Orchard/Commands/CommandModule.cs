using System.Linq;
using Autofac;
using Autofac.Core;

namespace Orchard.Commands {
    public class CommandModule : Module {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            if (!registration.Services.Contains(new TypedService(typeof(ICommandHandler))))
                return;

            var builder = new CommandHandlerDescriptorBuilder();
            var descriptor = builder.Build(registration.Activator.LimitType);
            registration.Metadata.Add(typeof(CommandHandlerDescriptor).FullName, descriptor);
        }
    }
}
