using System;
using System.Linq;
using Orchard.Localization;

namespace Orchard.Commands.Builtin {
    public class HelpCommand : DefaultOrchardCommandHandler {
        private readonly ICommandManager _commandManager;

        public HelpCommand(ICommandManager commandManager) {
            _commandManager = commandManager;
        }

        [CommandName("help commands")]
        [CommandHelp("help commands: Display help text for all available commands")]
        public void AllCommands() {
            Context.Output.WriteLine(T("List of available commands:"));
            Context.Output.WriteLine(T("---------------------------"));

            var descriptors = _commandManager.GetCommandDescriptors();
            foreach (var descriptor in descriptors) {
                Context.Output.WriteLine(GetHelpText(descriptor));
            }
        }

        private LocalizedString GetHelpText(CommandDescriptor descriptor) {
            return string.IsNullOrEmpty(descriptor.HelpText) ? T("[no help text]") : T(descriptor.HelpText);
        }

        [CommandName("help")]
        [CommandHelp("help <command>: Display help text for <command>")]
        public void SingleCommand(string[] commandNameStrings) {
            string command = string.Join(" ", commandNameStrings);
            var descriptor = _commandManager.GetCommandDescriptors().SingleOrDefault(d => string.Equals(command, d.Name, StringComparison.OrdinalIgnoreCase));
            if (descriptor == null) {
                Context.Output.WriteLine(T("Command {0} doesn't exist").ToString(), command);
            }
            else {
                Context.Output.WriteLine(GetHelpText(descriptor));
            }
        }
    }
}
