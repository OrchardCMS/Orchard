using System.Collections.Generic;
using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class BinaryAstNode : AstNode, IAstNodeWithToken
    {
        public BinaryAstNode(AstNode left, Token token, AstNode right)
        {
            Left = left;
            Operator = token;
            Right = right;
        }

        public Token Token => Operator;

        public Token Operator { get; }

        public override object Accept(AstVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override IEnumerable<AstNode> Children => new List<AstNode>(2) { Left, Right };

        public AstNode Left { get; }

        public AstNode Right { get; }
    }
}