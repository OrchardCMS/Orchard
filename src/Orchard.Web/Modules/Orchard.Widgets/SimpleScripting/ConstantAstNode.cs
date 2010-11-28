namespace Orchard.Widgets.SimpleScripting {
    public class ConstantAstNode : AstNode, IAstNodeWithToken {
        private readonly Terminal _terminal;

        public ConstantAstNode(Terminal terminal) {
            _terminal = terminal;
        }

        public Terminal Terminal {
            get { return _terminal; }
        }

        public object Value { get { return _terminal.Value; } }
    }
}