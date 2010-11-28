using System;
using System.Collections.Generic;

namespace Orchard.Widgets.SimpleScripting {
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

            var token = IsMatch(TerminalKind.Or);
            if (token != null)
            {
                var right = ParseKeywordOrExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseKeywordAndExpression() {
            var expr = ParseKeywordNotExpression();

            var token = IsMatch(TerminalKind.And);
            if (token != null) {
                var right = ParseKeywordAndExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseKeywordNotExpression() {
            var token = IsMatch(TerminalKind.Not);
            if (token != null) {
                var expr = ParseKeywordNotExpression();
                    
                return new UnaryAstNode(expr, token);
            }

            return ParseRelationalExpression();
        }

        private AstNode ParseRelationalExpression() {
            var expr = ParseAdditiveExpression();
            //TODO
            //var Terminal = IsMatch(TokenKind.Not);
            //if (Terminal != null) {
            return expr;
        }

        private AstNode ParseAdditiveExpression() {
            var expr = ParseMultiplicativeExpression();

            var token = IsMatch(TerminalKind.Plus, TerminalKind.Minus);
            if (token != null) {
                var right = ParseAdditiveExpression();

                expr = new BinaryAstNode(expr, token, right);
            }

            return expr;
        }

        private AstNode ParseMultiplicativeExpression() {
            var expr = ParseUnaryExpression();

            var token = IsMatch(TerminalKind.Mul, TerminalKind.Div);
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
                case TerminalKind.True:
                case TerminalKind.False:
                case TerminalKind.SingleQuotedStringLiteral:
                case TerminalKind.StringLiteral:
                case TerminalKind.Integer:
                    return ProduceConstant(token);
                case TerminalKind.OpenParen:
                    return ParseParenthesizedExpression();
                default:
                    return ProduceError(token);
            }
        }

        private AstNode ProduceConstant(Terminal terminal) {
            _lexer.NextToken();
            return new ConstantAstNode(terminal);
        }

        private AstNode ProduceError(Terminal terminal) {
            _lexer.NextToken();
            return new ErrorAstNode(terminal,
                                                      string.Format("Unexptected Terminal in primary expression ({0})", terminal));
        }

        private AstNode ParseParenthesizedExpression() {
            Match(TerminalKind.OpenParen);
            var expr = ParseExpression();
            Match(TerminalKind.CloseParen);
            return expr;
        }

        private void Match(TerminalKind kind) {
            var token = _lexer.Token();
            if (token.Kind == kind) {
                _lexer.NextToken();
                return;
            }
            AddError(token, string.Format("Expected Terminal {0}", kind));
        }

        private Terminal IsMatch(TerminalKind kind) {
            var token = _lexer.Token();
            if (token.Kind == kind) {
                _lexer.NextToken();
                return token;
            }
            return null;
        }

        private Terminal IsMatch(TerminalKind kind, TerminalKind kind2) {
            var token = _lexer.Token();
            if (token.Kind == kind || token.Kind == kind2) {
                _lexer.NextToken();
                return token;
            }
            return null;
        }

        private void AddError(Terminal terminal, string message) {
            _errors.Add(message);
        }
    }
}