using System;

namespace Orchard.Commands {
    [AttributeUsage(AttributeTargets.Method)]
    public class OrchardCommandAttribute : Attribute {
        private readonly string _commandAlias;

        public OrchardCommandAttribute(string commandAlias) {
            _commandAlias = commandAlias;
        }

        public string Command {
            get { return _commandAlias; }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandHelpAttribute : Attribute {
        public CommandHelpAttribute(string text) {
            this.HelpText = text;
        }

        public string HelpText { get; set; }
    }
}
