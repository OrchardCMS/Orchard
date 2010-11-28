using System;
using System.Collections.Generic;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Widgets.SimpleScripting.Ast {
    public class BinaryAstNode : AstNode, IAstNodeWithToken {
        private readonly AstNode _left;
        private readonly Token _token;
        private readonly AstNode _right;

        public BinaryAstNode(AstNode left, Token token, AstNode right) {
            _left = left;
            _token = token;
            _right = right;
        }

        public Token Token {
            get { return _token; }
        }

        public Token Operator {
            get { return _token; }
        }

        public override object Accept(AstVisitor visitor) {
            return visitor.VisitBinary(this);
        }

        public override IEnumerable<AstNode> Children {
            get {
                yield return _left;
                yield return _right;
            }
        }

        public AstNode Left { get { return _left; } }

        public AstNode Right { get { return _right; } }
    }
}