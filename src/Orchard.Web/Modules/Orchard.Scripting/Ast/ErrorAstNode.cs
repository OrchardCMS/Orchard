using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class ErrorAstNode : AstNode, IAstNodeWithToken
    {
        public ErrorAstNode(Token token, string message)
        {
            Token = token;
            Message = message;
        }

        public Token Token { get; }

        public string Message { get; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", GetType().Name, Message);
        }

        public override object Accept(AstVisitor visitor)
        {
            return visitor.VisitError(this);
        }
    }
}