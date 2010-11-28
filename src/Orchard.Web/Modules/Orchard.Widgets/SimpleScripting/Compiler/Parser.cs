using System.Collections.Generic;
using Orchard.Widgets.SimpleScripting.Ast;

namespace Orchard.Widgets.SimpleScripting.Compiler {
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
            return ParseKeywordOrExpression();
        }

        private AstNode ParseKeywordOrExpression() {
            var expr = ParseKeywordAndExpression();

            var token = IsMatch(TokenKind.Or);
            if (token != null)
            {
                var right = ParseKeywordOrExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseKeywordAndExpression() {
            var expr = ParseKeywordNotExpression();

            var token = IsMatch(TokenKind.And);
            if (token != null) {
                var right = ParseKeywordAndExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseKeywordNotExpression() {
            var token = IsMatch(TokenKind.Not);
            if (token != null) {
                var expr = ParseKeywordNotExpression();
                    
                return new UnaryAstNode(expr, token);
            }

            return ParseRelationalExpression();
        }

        private AstNode ParseRelationalExpression() {
            var expr = ParseAdditiveExpression();
            //TODO
            //var Token = IsMatch(TokenKind.Not);
            //if (Token != null) {
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
            //TODO
            return ParsePrimaryExpression();
        }

        private AstNode ParsePrimaryExpression() {
            var token = _lexer.Token();
            switch(_lexer.Token().Kind) {
                case TokenKind.True:
                case TokenKind.False:
                case TokenKind.SingleQuotedStringLiteral:
                case TokenKind.StringLiteral:
                case TokenKind.Integer:
                    return ProduceConstant(token);
                case TokenKind.OpenParen:
                    return ParseParenthesizedExpression();
                default:
                    return ProduceError(token);
            }
        }

        private AstNode ProduceConstant(Token token) {
            _lexer.NextToken();
            return new ConstantAstNode(token);
        }

        private AstNode ProduceError(Token token) {
            _lexer.NextToken();
            return new ErrorAstNode(token,
                                                      string.Format("Unexptected Token in primary expression ({0})", token));
        }

        private AstNode ParseParenthesizedExpression() {
            Match(TokenKind.OpenParen);
            var expr = ParseExpression();
            Match(TokenKind.CloseParen);
            return expr;
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