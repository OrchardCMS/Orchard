using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Widgets.SimpleScripting.Ast {
    public class ConstantAstNode : AstNode, IAstNodeWithToken {
        private readonly Token _token;

        public ConstantAstNode(Token token) {
            _token = token;
        }

        public Token Token {
            get { return _token; }
        }

        public object Value { get { return _token.Value; } }
    }
}