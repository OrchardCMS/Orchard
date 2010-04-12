using System;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Metadata;

namespace Orchard.Commands.Builtin {
    public class HelpCommand : DefaultOrchardCommandHandler {
        private readonly ICommandManager _commandManager;

        public HelpCommand(ICommandManager commandManager) {
            _commandManager = commandManager;
        }

        [OrchardCommand("help commands")]
        [CommandHelp("List all available commands")]
        public void Commands() {
            var descriptors = _commandManager.GetCommandDescriptors();
            foreach (var descriptor in descriptors) {
                var helpText = string.IsNullOrEmpty(descriptor.HelpText) ? T("[no help text]") : T(descriptor.HelpText);
                Context.Output.WriteLine("{0}: {1}", descriptor.Name, helpText);
            }
        }
    }
}
