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
        [CommandHelp("help commands\r\n\t" + "Display help text for all available commands")]
        public void AllCommands() {
            Context.Output.WriteLine(T("List of available commands:"));
            Context.Output.WriteLine(T("---------------------------"));
            Context.Output.WriteLine();

            var descriptors = _commandManager.GetCommandDescriptors().OrderBy(d => d.Name);
            foreach (var descriptor in descriptors) {
                Context.Output.WriteLine(GetHelpText(descriptor));
                Context.Output.WriteLine();
            }
        }

        private LocalizedString GetHelpText(CommandDescriptor descriptor) {
            if (string.IsNullOrEmpty(descriptor.HelpText)) {
                return T("{0}: no help text",
                         descriptor.MethodInfo.DeclaringType.FullName + "." + descriptor.MethodInfo.Name);
            }

            return T(descriptor.HelpText);
        }

        [CommandName("help")]
        [CommandHelp("help <command>\r\n\t" + "Display help text for <command>")]
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
