using System.Collections.Generic;
using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class BinaryAstNode : AstNode, IAstNodeWithToken
    {
        private readonly AstNode _left;
        private readonly Token _token;
        private readonly AstNode _right;

        public BinaryAstNode(AstNode left, Token token, AstNode right)
        {
            _left = left;
            _token = token;
            _right = right;
        }

        public Token Token => _token;

        public Token Operator => _token;

        public override object Accept(AstVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override IEnumerable<AstNode> Children => new List<AstNode>(2) { _left, _right };

        public AstNode Left => _left;

        public AstNode Right => _right;
    }
}