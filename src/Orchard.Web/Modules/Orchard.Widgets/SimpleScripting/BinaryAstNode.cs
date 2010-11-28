using System.Collections.Generic;

namespace Orchard.Widgets.SimpleScripting {
    public class BinaryAstNode : AstNode, IAstNodeWithToken {
        private readonly AstNode _left;
        private readonly Terminal _terminal;
        private readonly AstNode _right;

        public BinaryAstNode(AstNode left, Terminal terminal, AstNode right) {
            _left = left;
            _terminal = terminal;
            _right = right;
        }

        public Terminal Terminal {
            get { return _terminal; }
        }

        public Terminal Operator {
            get { return _terminal; }
        }

        public override IEnumerable<AstNode> Children {
            get {
                yield return _left;
                yield return _right;
            }
        }
    }
}