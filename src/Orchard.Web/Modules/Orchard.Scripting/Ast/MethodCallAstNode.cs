using System.Collections.Generic;
using Orchard.Scripting.Compiler;

namespace Orchard.Scripting.Ast
{
    public class MethodCallAstNode : AstNode, IAstNodeWithToken
    {
        public MethodCallAstNode(Token token, IList<AstNode> arguments)
        {
            Token = token;
            Arguments = arguments;
        }

        public Token Target => Token;
        public IList<AstNode> Arguments { get; }

        public Token Token { get; }

        public override IEnumerable<AstNode> Children => Arguments;

        public override object Accept(AstVisitor visitor)
        {
            return visitor.VisitMethodCall(this);
        }
    }
}