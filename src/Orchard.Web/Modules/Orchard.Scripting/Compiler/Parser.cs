using System.Collections.Generic;
using Orchard.Scripting.Ast;

namespace Orchard.Scripting.Compiler {
    public class Parser {
        private readonly string _expression;
        private readonly Lexer _lexer;
        private readonly List<string> _errors = new List<string>();

        public Parser(string expression) {
            _expression = expression;
            _lexer = new Lexer(new Tokenizer(_expression));
        }

        public AbstractSyntaxTree Parse() {
            var node = ParseExpression();
            return new AbstractSyntaxTree { Root = node };
        }

        private AstNode ParseExpression() {
            return ParseKeywordLogicalExpression();
        }

        private AstNode ParseKeywordLogicalExpression() {
            var expr = ParseKeywordNotExpression();

            var token = IsMatch(TokenKind.Or, TokenKind.And);
            if (token != null) {
                var right = ParseKeywordLogicalExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseKeywordNotExpression() {
            var token = IsMatch(TokenKind.Not);
            if (token != null) {
                var expr = ParseKeywordNotExpression();

                return new UnaryAstNode(token, expr);
            }

            return ParseEqualityExpression();
        }

        private AstNode ParseEqualityExpression() {
            var expr = ParseRelationalExpression();

            var token = IsMatch(TokenKind.EqualEqual, TokenKind.NotEqual);
            if (token != null) {
                var right = ParseEqualityExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseRelationalExpression() {
            var expr = ParseAdditiveExpression();

            var token = 
                IsMatch(TokenKind.LessThan, TokenKind.LessThanEqual) ??
                IsMatch(TokenKind.GreaterThan, TokenKind.GreaterThanEqual);
            if (token != null) {
                var right = ParseRelationalExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseAdditiveExpression() {
            var expr = ParseMultiplicativeExpression();

            var token = IsMatch(TokenKind.Plus, TokenKind.Minus);
            if (token != null) {
                var right = ParseAdditiveExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseMultiplicativeExpression() {
            var expr = ParseUnaryExpression();

            var token = IsMatch(TokenKind.Mul, TokenKind.Div);
            if (token != null) {
                var right = ParseMultiplicativeExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseUnaryExpression() {
            var token = IsMatch(TokenKind.NotSign);
            if (token != null) {
                var expr = ParseUnaryExpression();

                return new UnaryAstNode(token, expr);
            }

            return ParsePrimaryExpression();
        }

        private AstNode ParsePrimaryExpression() {
            var token = _lexer.Token();
            switch (_lexer.Token().Kind) {
                case TokenKind.True:
                case TokenKind.False:
                case TokenKind.SingleQuotedStringLiteral:
                case TokenKind.StringLiteral:
                case TokenKind.Integer:
                    return ProduceConstant(token);
                case TokenKind.OpenParen:
                    return ParseParenthesizedExpression();
                case TokenKind.Identifier:
                    return ParseMethodCallExpression();
                default:
                    return ProduceError(token);
            }
        }

        private AstNode ParseParenthesizedExpression() {
            Match(TokenKind.OpenParen);
            var expr = ParseExpression();
            Match(TokenKind.CloseParen);
            return expr;
        }

        private AstNode ParseMethodCallExpression() {
            var target = _lexer.Token();
            _lexer.NextToken();

            bool isParenthesizedCall = (IsMatch(TokenKind.OpenParen) != null);

            var arguments = new List<AstNode>();
            while (true) {
                // Special case: we might reach the end of the token stream
                if (_lexer.Token().Kind == TokenKind.Eof)
                    break;

                // Special case: we must support "foo()"
                if (isParenthesizedCall && _lexer.Token().Kind == TokenKind.CloseParen)
                    break;

                var argument = ParseExpression();
                arguments.Add(argument);

                if (IsMatch(TokenKind.Comma) == null)
                    break;
            }

            if (isParenthesizedCall)
                Match(TokenKind.CloseParen);

            return new MethodCallAstNode(target, arguments);
        }

        private AstNode ProduceConstant(Token token) {
            _lexer.NextToken();
            return new ConstantAstNode(token);
        }

        private AstNode ProduceError(Token token) {
            _lexer.NextToken();
            return new ErrorAstNode(token, string.Format("Unexptected Token in primary expression ({0})", token));
        }

        private void Match(TokenKind kind) {
            var token = _lexer.Token();
            if (token.Kind == kind) {
                _lexer.NextToken();
                return;
            }
            AddError(token, string.Format("Expected Token {0}", kind));
        }

        private Token IsMatch(TokenKind kind) {
            var token = _lexer.Token();
            if (token.Kind == kind) {
                _lexer.NextToken();
                return token;
            }
            return null;
        }

        private Token IsMatch(TokenKind kind, TokenKind kind2) {
            var token = _lexer.Token();
            if (token.Kind == kind || token.Kind == kind2) {
                _lexer.NextToken();
                return token;
            }
            return null;
        }

        private void AddError(Token token, string message) {
            _errors.Add(message);
        }
    }
}