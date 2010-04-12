using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac;
using Autofac.Core;

namespace Orchard.Commands {
    public class CommandModule : Module {
        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {

            if (!registration.Services.Contains(new TypedService(typeof(ICommandHandler))))
                return;

            // Workaround autofac integration: module registration is currently run twice...
            if (!registration.Metadata.ContainsKey(typeof(CommandHandlerDescriptor).FullName)) {
                var builder = new CommandHandlerDescriptorBuilder();
                var descriptor = builder.Build(registration.Activator.LimitType);
                registration.Metadata.Add(typeof (CommandHandlerDescriptor).FullName, descriptor);
            }
        }
    }
}
