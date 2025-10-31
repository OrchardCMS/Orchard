using System.Collections.Generic;
using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class UnaryAstNode : AstNode, IAstNodeWithToken
    {
        public UnaryAstNode(Token token, AstNode operand)
        {
            Operand = operand;
            Operator = token;
        }

        public Token Token => Operator;
        public Token Operator { get; }
        public AstNode Operand { get; }

        public override IEnumerable<AstNode> Children => new List<AstNode>(1) { Operand };

        public override object Accept(AstVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }
    }
}