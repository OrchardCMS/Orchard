using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class ConstantAstNode : AstNode, IAstNodeWithToken
    {
        public ConstantAstNode(Token token)
        {
            Token = token;
        }

        public Token Token { get; }

        public object Value => Token.Value;

        public override object Accept(AstVisitor visitor)
        {
            return visitor.VisitConstant(this);
        }
    }
}