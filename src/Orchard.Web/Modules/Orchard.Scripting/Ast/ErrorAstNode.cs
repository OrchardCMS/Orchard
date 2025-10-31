using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class ErrorAstNode : AstNode, IAstNodeWithToken
    {
        private readonly Token _token;
        private readonly string _message;

        public ErrorAstNode(Token token, string message)
        {
            _token = token;
            _message = message;
        }

        public Token Token => _token;

        public string Message => _message;

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