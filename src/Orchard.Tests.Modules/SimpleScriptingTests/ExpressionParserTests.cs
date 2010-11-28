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
                typeof(ExpressionTree.ContantExpression), true,
            });
        }

        [Test]
        public void ParserShouldUnderstandBinaryExpressions() {
            var tree = new ExpressionParser("true+true").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.BinaryExpression), TokenKind.Plus,
                    typeof(ExpressionTree.ContantExpression), true,
                    typeof(ExpressionTree.ContantExpression), true,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence() {
            var tree = new ExpressionParser("1+2*3").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.BinaryExpression), TokenKind.Plus,
                    typeof(ExpressionTree.ContantExpression), 1,
                    typeof(ExpressionTree.BinaryExpression), TokenKind.Mul,
                        typeof(ExpressionTree.ContantExpression), 2,
                        typeof(ExpressionTree.ContantExpression), 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence2() {
            var tree = new ExpressionParser("1*2+3").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.BinaryExpression), TokenKind.Plus,
                    typeof(ExpressionTree.BinaryExpression), TokenKind.Mul,
                        typeof(ExpressionTree.ContantExpression), 1,
                        typeof(ExpressionTree.ContantExpression), 2,
                    typeof(ExpressionTree.ContantExpression), 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence3() {
            var tree = new ExpressionParser("not true or true").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.BinaryExpression), TokenKind.Or,
                    typeof(ExpressionTree.UnaryExpression), TokenKind.Not,
                        typeof(ExpressionTree.ContantExpression), true,
                    typeof(ExpressionTree.ContantExpression), true,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence4() {
            var tree = new ExpressionParser("not (true or true)").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.UnaryExpression), TokenKind.Not,
                  typeof(ExpressionTree.BinaryExpression), TokenKind.Or,
                    typeof(ExpressionTree.ContantExpression), true,
                    typeof(ExpressionTree.ContantExpression), true,
            });
        }

        [Test]
        public void ParserShouldUnderstandParenthesis() {
            var tree = new ExpressionParser("1*(2+3)").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.BinaryExpression), TokenKind.Mul,
                    typeof(ExpressionTree.ContantExpression), 1,
                    typeof(ExpressionTree.BinaryExpression), TokenKind.Plus,
                        typeof(ExpressionTree.ContantExpression), 2,
                        typeof(ExpressionTree.ContantExpression), 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandComplexExpressions() {
            var tree = new ExpressionParser("not 1 * (2 / 4 * 6 + (3))").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.UnaryExpression), TokenKind.Not,
                    typeof(ExpressionTree.BinaryExpression), TokenKind.Mul,
                        typeof(ExpressionTree.ContantExpression), 1,
                        typeof(ExpressionTree.BinaryExpression), TokenKind.Plus,
                            typeof(ExpressionTree.BinaryExpression), TokenKind.Div,
                                typeof(ExpressionTree.ContantExpression), 2,
                                typeof(ExpressionTree.BinaryExpression), TokenKind.Mul,
                                    typeof(ExpressionTree.ContantExpression), 4,
                                    typeof(ExpressionTree.ContantExpression), 6,
                            typeof(ExpressionTree.ContantExpression), 3,
            });
        }

        [Test]
        public void ParserShouldContainErrorExpressions() {
            var tree = new ExpressionParser("1 + not 3").Parse();
            CheckTree(tree, new object[] {
                typeof(ExpressionTree.BinaryExpression), TokenKind.Plus,
                    typeof(ExpressionTree.ContantExpression), 1,
                    typeof(ExpressionTree.ErrorExpression),
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
            var type = (Type)objects[index++];

            Trace.WriteLine(string.Format("{0}: {1}{2} (Current: {3})", indent, new string(' ', indent * 2), type.Name, expression));

            Assert.That(expression.GetType(), Is.EqualTo(type));

            if (type == typeof(ExpressionTree.ContantExpression)) {
                Assert.That((expression as ExpressionTree.ContantExpression).Value, Is.EqualTo(objects[index++]));
            }
            else if (type == typeof(ExpressionTree.BinaryExpression)) {
                Assert.That((expression as ExpressionTree.BinaryExpression).Operator.Kind, Is.EqualTo(objects[index++]));
            }
            else if (type == typeof(ExpressionTree.UnaryExpression)) {
                Assert.That((expression as ExpressionTree.UnaryExpression).Operator.Kind, Is.EqualTo(objects[index++]));
            }

            foreach(var child in expression.Children) {
                CheckExpression(child, indent + 1, objects, ref index);
            }
        }
    }
}
