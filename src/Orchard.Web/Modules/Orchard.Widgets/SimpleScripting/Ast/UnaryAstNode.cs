using System.Collections.Generic;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Widgets.SimpleScripting.Ast {
    public class UnaryAstNode : AstNode, IAstNodeWithToken {
        private readonly AstNode _expr;
        private readonly Token _token;

        public UnaryAstNode(AstNode expr, Token token) {
            _expr = expr;
            _token = token;
        }

        public Token Token {
            get { return _token; }
        }


        public Token Operator {
            get { return _token; }
        }

        public override IEnumerable<AstNode> Children {
            get {
                yield return _expr;
            }
        }

        public override object Accept(AstVisitor visitor) {
            return visitor.VisitUnary(this);
        }
    }
}