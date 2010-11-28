using System;
using System.Collections.Generic;

namespace Orchard.Widgets.SimpleScripting {
    public class ExpressionParser {
        private readonly string _expression;
        private readonly ExpressionLexer _lexer;
        private readonly List<string> _errors = new List<string>();

        public ExpressionParser(string expression) {
            _expression = expression;
            _lexer = new ExpressionLexer(new ExpressionTokenizer(_expression));
        }

        public ExpressionTree Parse() {
            var node = ParseExpression();
            return new ExpressionTree { Root = node };
        }

        private ExpressionTree.Expression ParseExpression() {
            return ParseKeywordLogicalExpression();
        }

        private ExpressionTree.Expression ParseKeywordLogicalExpression() {
            return ParseKeywordOrExpression();
        }

        private ExpressionTree.Expression ParseKeywordOrExpression() {
            var expr = ParseKeywordAndExpression();

            var token = IsMatch(TokenKind.Or);
            if (token != null)
            {
                var right = ParseKeywordOrExpression();

                expr = new ExpressionTree.BinaryExpression(expr, token, right);
            }

            return expr;
        }

        private ExpressionTree.Expression ParseKeywordAndExpression() {
            var expr = ParseKeywordNotExpression();

            var token = IsMatch(TokenKind.And);
            if (token != null) {
                var right = ParseKeywordAndExpression();

                expr = new ExpressionTree.BinaryExpression(expr, token, right);
            }

            return expr;
        }

        private ExpressionTree.Expression ParseKeywordNotExpression() {
            var token = IsMatch(TokenKind.Not);
            if (token != null) {
                var expr = ParseKeywordNotExpression();
                    
                return new ExpressionTree.UnaryExpression(expr, token);
            }

            return ParseRelationalExpression();
        }

        private ExpressionTree.Expression ParseRelationalExpression() {
            var expr = ParseAdditiveExpression();
            //TODO
            //var token = IsMatch(TokenKind.Not);
            //if (token != null) {
            return expr;
        }

        private ExpressionTree.Expression ParseAdditiveExpression() {
            var expr = ParseMultiplicativeExpression();

            var token = IsMatch(TokenKind.Plus, TokenKind.Minus);
            if (token != null) {
                var right = ParseAdditiveExpression();

                expr = new ExpressionTree.BinaryExpression(expr, token, right);
            }

            return expr;
        }

        private ExpressionTree.Expression ParseMultiplicativeExpression() {
            var expr = ParseUnaryExpression();

            var token = IsMatch(TokenKind.Mul, TokenKind.Div);
            if (token != null) {
                var right = ParseMultiplicativeExpression();

                expr = new ExpressionTree.BinaryExpression(expr, token, right);
            }

            return expr;
        }

        private ExpressionTree.Expression ParseUnaryExpression() {
            //TODO
            return ParsePrimaryExpression();
        }

        private ExpressionTree.Expression ParsePrimaryExpression() {
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

        private ExpressionTree.Expression ProduceConstant(ExpressionTokenizer.Token token) {
            _lexer.NextToken();
            return new ExpressionTree.ConstantExpression(token);
        }

        private ExpressionTree.Expression ProduceError(ExpressionTokenizer.Token token) {
            _lexer.NextToken();
            return new ExpressionTree.ErrorExpression(token,
                                                      string.Format("Unexptected token in primary expression ({0})", token));
        }

        private ExpressionTree.Expression ParseParenthesizedExpression() {
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
            AddError(token, string.Format("Expected token {0}", kind));
        }

        private ExpressionTokenizer.Token IsMatch(TokenKind kind) {
            var token = _lexer.Token();
            if (token.Kind == kind) {
                _lexer.NextToken();
                return token;
            }
            return null;
        }

        private ExpressionTokenizer.Token IsMatch(TokenKind kind, TokenKind kind2) {
            var token = _lexer.Token();
            if (token.Kind == kind || token.Kind == kind2) {
                _lexer.NextToken();
                return token;
            }
            return null;
        }

        private void AddError(ExpressionTokenizer.Token token, string message) {
            _errors.Add(message);
        }
    }
}