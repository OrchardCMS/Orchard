using System;
using System.Diagnostics;
using NUnit.Framework;
using Orchard.Widgets.SimpleScripting;

namespace Orchard.Tests.Modules.SimpleScriptingTests {
    [TestFixture]
    public class ExpressionParserTests {
        [Test]
        public void ParserShouldUnderstandConstantExpressions() {
            var tree = new ExpressionParser("true").Parse();
            CheckTree(tree, new object[] {
                "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandBinaryExpressions() {
            var tree = new ExpressionParser("true+true").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "const", true,
                    "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence() {
            var tree = new ExpressionParser("1+2*3").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "const", 1,
                    "binop", TokenKind.Mul,
                        "const", 2,
                        "const", 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence2() {
            var tree = new ExpressionParser("1*2+3").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "binop", TokenKind.Mul,
                        "const", 1,
                        "const", 2,
                    "const", 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence3() {
            var tree = new ExpressionParser("not true or true").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Or,
                    "unop", TokenKind.Not,
                        "const", true,
                    "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence4() {
            var tree = new ExpressionParser("not (true or true)").Parse();
            CheckTree(tree, new object[] {
                "unop", TokenKind.Not,
                  "binop", TokenKind.Or,
                    "const", true,
                    "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandParenthesis() {
            var tree = new ExpressionParser("1*(2+3)").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Mul,
                    "const", 1,
                    "binop", TokenKind.Plus,
                        "const", 2,
                        "const", 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandComplexExpressions() {
            var tree = new ExpressionParser("not 1 * (2 / 4 * 6 + (3))").Parse();
            CheckTree(tree, new object[] {
                "unop", TokenKind.Not,
                    "binop", TokenKind.Mul,
                        "const", 1,
                        "binop", TokenKind.Plus,
                            "binop", TokenKind.Div,
                                "const", 2,
                                "binop", TokenKind.Mul,
                                    "const", 4,
                                    "const", 6,
                            "const", 3,
            });
        }

        [Test]
        public void ParserShouldContainErrorExpressions() {
            var tree = new ExpressionParser("1 + not 3").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "const", 1,
                    "error",
            });
        }

        private void CheckTree(ExpressionTree tree, object[] objects) {
            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.Root, Is.Not.Null);

            int index = 0;
            CheckExpression(tree.Root, 0, objects, ref index);
            Assert.That(index, Is.EqualTo(objects.Length));
        }

        private void CheckExpression(ExpressionTree.Expression expression, int indent, object[] objects, ref int index) {
            var exprName = (string)objects[index++];
            Type type = null;
            switch(exprName) {
                case "const":
                    type = typeof(ExpressionTree.ConstantExpression);
                    break;
                case "binop":
                    type = typeof(ExpressionTree.BinaryExpression);
                    break;
                case "unop":
                    type = typeof(ExpressionTree.UnaryExpression);
                    break;
                case "error":
                    type = typeof(ExpressionTree.ErrorExpression);
                    break;
            }

            Trace.WriteLine(string.Format("{0}: {1}{2} (Current: {3})", indent, new string(' ', indent * 2), type.Name, expression));

            Assert.That(expression.GetType(), Is.EqualTo(type));

            if (exprName == "const") {
                Assert.That((expression as ExpressionTree.ConstantExpression).Value, Is.EqualTo(objects[index++]));
            }
            else if (exprName == "binop") {
                Assert.That((expression as ExpressionTree.BinaryExpression).Operator.Kind, Is.EqualTo(objects[index++]));
            }
            else if (exprName == "unop") {
                Assert.That((expression as ExpressionTree.UnaryExpression).Operator.Kind, Is.EqualTo(objects[index++]));
            }

            foreach(var child in expression.Children) {
                CheckExpression(child, indent + 1, objects, ref index);
            }
        }
    }
}
