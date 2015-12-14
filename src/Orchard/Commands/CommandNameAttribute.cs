using System;

namespace Orchard.Commands {
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandNameAttribute : Attribute {
        private readonly string _commandAlias;

        public CommandNameAttribute(string commandAlias) {
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
