using System.Collections.Generic;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Widgets.SimpleScripting.Ast {
    public class UnaryAstNode : AstNode, IAstNodeWithToken {
        private readonly AstNode _operand;
        private readonly Token _token;

        public UnaryAstNode(AstNode operand, Token token) {
            _operand = operand;
            _token = token;
        }

        public Token Token { get { return _token; } }
        public Token Operator { get { return _token; } }
        public AstNode Operand { get { return _operand; } }

        public override IEnumerable<AstNode> Children {
            get { yield return _operand; }
        }

        public override object Accept(AstVisitor visitor) {
            return visitor.VisitUnary(this);
        }
    }
}