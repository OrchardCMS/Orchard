using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orchard.Widgets.SimpleScripting {
    public class ExpressionTree {
        public Expression Root { get; set; }

        public interface IExpressionWithToken {
            ExpressionTokenizer.Token Token { get; }
        }

        public class Expression {
            public virtual IEnumerable<Expression> Children {
                get {
                    return Enumerable.Empty<Expression>();
                }
            }

            public override string ToString() {
                var sb = new StringBuilder();
                sb.Append(this.GetType().Name);
                var ewt = (this as IExpressionWithToken);
                if (ewt != null) {
                    sb.Append(" - ");
                    sb.Append(ewt.Token.Kind);
                    if (ewt.Token.Value != null) {
                        sb.Append(" - ");
                        sb.Append(ewt.Token.Value);
                    }
                }
                return sb.ToString();
            }
        }

        public class ErrorExpression : Expression, IExpressionWithToken {
            private readonly ExpressionTokenizer.Token _token;
            private readonly string _message;

            public ErrorExpression(ExpressionTokenizer.Token token, string message) {
                _token = token;
                _message = message;
            }

            public ExpressionTokenizer.Token Token {
                get { return _token; }
            }

            public string Message {
                get { return _message; }
            }

            public override string ToString() {
                return string.Format("{0} - {1}", GetType().Name, Message);
            }
        }


        public class ContantExpression : Expression, IExpressionWithToken {
            private readonly ExpressionTokenizer.Token _token;

            public ContantExpression(ExpressionTokenizer.Token token) {
                _token = token;
            }

            public ExpressionTokenizer.Token Token {
                get { return _token; }
            }

            public object Value { get { return _token.Value; } }
        }

        public class BinaryExpression : Expression, IExpressionWithToken {
            private readonly Expression _left;
            private readonly ExpressionTokenizer.Token _token;
            private readonly Expression _right;

            public BinaryExpression(Expression left, ExpressionTokenizer.Token token, Expression right) {
                _left = left;
                _token = token;
                _right = right;
            }

            public ExpressionTokenizer.Token Token {
                get { return _token; }
            }

            public ExpressionTokenizer.Token Operator {
                get { return _token; }
            }

            public override IEnumerable<Expression> Children {
                get {
                    yield return _left;
                    yield return _right;
                }
            }
        }

        public class UnaryExpression : Expression, IExpressionWithToken {
            private readonly Expression _expr;
            private readonly ExpressionTokenizer.Token _token;

            public UnaryExpression(Expression expr, ExpressionTokenizer.Token token) {
                _expr = expr;
                _token = token;
            }

            public ExpressionTokenizer.Token Token {
                get { return _token; }
            }


            public ExpressionTokenizer.Token Operator {
                get { return _token; }
            }

            public override IEnumerable<Expression> Children {
                get {
                    yield return _expr;
                }
            }
        }
    }
}