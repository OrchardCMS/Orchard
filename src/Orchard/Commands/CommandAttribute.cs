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
}
