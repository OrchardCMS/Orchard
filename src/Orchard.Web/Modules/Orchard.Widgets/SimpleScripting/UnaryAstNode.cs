using System.Collections.Generic;

namespace Orchard.Widgets.SimpleScripting {
    public class UnaryAstNode : AstNode, IAstNodeWithToken {
        private readonly AstNode _expr;
        private readonly Terminal _terminal;

        public UnaryAstNode(AstNode expr, Terminal terminal) {
            _expr = expr;
            _terminal = terminal;
        }

        public Terminal Terminal {
            get { return _terminal; }
        }


        public Terminal Operator {
            get { return _terminal; }
        }

        public override IEnumerable<AstNode> Children {
            get {
                yield return _expr;
            }
        }
    }
}