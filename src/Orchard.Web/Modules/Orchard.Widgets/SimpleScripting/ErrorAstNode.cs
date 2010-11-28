using System;

namespace Orchard.Widgets.SimpleScripting {
    public class ErrorAstNode : AstNode, IAstNodeWithToken {
        private readonly Terminal _terminal;
        private readonly string _message;

        public ErrorAstNode(Terminal terminal, string message) {
            _terminal = terminal;
            _message = message;
        }

        public Terminal Terminal {
            get { return _terminal; }
        }

        public string Message {
            get { return _message; }
        }

        public override string ToString() {
            return String.Format("{0} - {1}", GetType().Name, Message);
        }
    }
}