using System;
using System.Diagnostics;
using NUnit.Framework;
using Orchard.Scripting.Ast;
using Orchard.Scripting.Compiler;

namespace Orchard.Tests.Modules.Scripting {
    [TestFixture]
    public class ParserTests {
        [Test]
        public void ParserShouldUnderstandConstantExpressions() {
            var tree = new Parser("true").Parse();
            CheckTree(tree, new object[] {
                "const", true,
            });
        }

        [Test]
        public void ParserShouldIgnoreWhitespaces() {
            var tree = new Parser("  true \n  ").Parse();
            CheckTree(tree, new object[] {
                "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandBinaryExpressions() {
            var tree = new Parser("true+true").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "const", true,
                    "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandCommandExpressions() {
            var tree = new Parser("print 'foo', 'bar'").Parse();
            CheckTree(tree, new object[] {
                "call", TokenKind.Identifier, "print",
                    "const", "foo",
                    "const", "bar",
            });
        }

        [Test]
        public void ParserShouldUnderstandCallExpressions() {
            var tree = new Parser("print('foo', 'bar')").Parse();
            CheckTree(tree, new object[] {
                "call", TokenKind.Identifier, "print",
                    "const", "foo",
                    "const", "bar",
            });
        }

        [Test]
        public void ParserShouldUnderstandCallExpressions2() {
            var tree = new Parser("print 1+2").Parse();
            CheckTree(tree, new object[] {
                "call", TokenKind.Identifier, "print",
                    "binop", TokenKind.Plus,
                        "const", 1,
                        "const", 2,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence() {
            var tree = new Parser("1+2*3").Parse();
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
            var tree = new Parser("1*2+3").Parse();
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
            var tree = new Parser("not true or true").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Or,
                    "unop", TokenKind.Not,
                        "const", true,
                    "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence4() {
            var tree = new Parser("not (true or true)").Parse();
            CheckTree(tree, new object[] {
                "unop", TokenKind.Not,
                  "binop", TokenKind.Or,
                    "const", true,
                    "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence5() {
            var tree = new Parser("1+2+3").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "binop", TokenKind.Plus,
                        "const", 1,
                        "const", 2,
                    "const", 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandOperatorPrecedence6() {
            var tree = new Parser("1+2-3").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Minus,
                    "binop", TokenKind.Plus,
                        "const", 1,
                        "const", 2,
                    "const", 3,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators() {
            var tree = new Parser("true == true").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.EqualEqual,
                  "const", true,
                  "const", true,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators2() {
            var tree = new Parser("1 != 2").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.NotEqual,
                  "const", 1,
                  "const", 2,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators3() {
            var tree = new Parser("1 < 2").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.LessThan,
                  "const", 1,
                  "const", 2,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators4() {
            var tree = new Parser("1 <= 2").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.LessThanEqual,
                  "const", 1,
                  "const", 2,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators5() {
            var tree = new Parser("1 > 2").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.GreaterThan,
                  "const", 1,
                  "const", 2,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators6() {
            var tree = new Parser("1 >= 2").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.GreaterThanEqual,
                  "const", 1,
                  "const", 2,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperators7() {
            var tree = new Parser("null == null").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.EqualEqual,
                  "const", null,
                  "const", null,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperatorPrecedence() {
            var tree = new Parser("1 < 2 or 2 > 3 and !false").Parse();
            CheckTree(tree, new object[] {
                  "binop", TokenKind.And,
                    "binop", TokenKind.Or,
                      "binop", TokenKind.LessThan,
                        "const", 1,
                        "const", 2,
                      "binop", TokenKind.GreaterThan,
                        "const", 2,
                        "const", 3,
                    "unop", TokenKind.NotSign,
                      "const", false,
            });
        }

        [Test]
        public void ParserShouldUnderstandRelationalOperatorPrecedence2() {
            var tree = new Parser("1 < 2 and 2 > 3 or !false").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Or,
                  "binop", TokenKind.And,
                    "binop", TokenKind.LessThan,
                        "const", 1,
                        "const", 2,
                      "binop", TokenKind.GreaterThan,
                        "const", 2,
                        "const", 3,
                    "unop", TokenKind.NotSign,
                      "const", false,
            });
        }

        [Test]
        public void ParserShouldUnderstandParenthesis() {
            var tree = new Parser("1*(2+3)").Parse();
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
            var tree = new Parser("not 1 * (2 / 4 * 6 + (3))").Parse();
            CheckTree(tree, new object[] {
                "unop", TokenKind.Not,
                    "binop", TokenKind.Mul,
                        "const", 1,
                        "binop", TokenKind.Plus,
                            "binop", TokenKind.Mul,
                                "binop", TokenKind.Div,
                                    "const", 2,
                                    "const", 4,
                                "const", 6,
                            "const", 3,
            });
        }

        [Test]
        public void ParserShouldContainErrorExpressions() {
            var tree = new Parser("1 + not 3").Parse();
            CheckTree(tree, new object[] {
                "error",
            });
        }

        [Test]
        public void ParserShouldContainErrorExpressions2() {
            var tree = new Parser("1 +").Parse();
            CheckTree(tree, new object[] {
                "binop", TokenKind.Plus,
                    "const", 1,
                    "error",
            });
        }

        private void CheckTree(AbstractSyntaxTree tree, object[] objects) {
            Assert.That(tree, Is.Not.Null);
            Assert.That(tree.Root, Is.Not.Null);

            int index = 0;
            CheckExpression(tree.Root, 0, objects, ref index);
            Assert.That(index, Is.EqualTo(objects.Length));
        }

        private void CheckExpression(AstNode astNode, int indent, object[] objects, ref int index) {
            var exprName = (string)objects[index++];
            Type type = null;
            switch (exprName) {
                case "const":
                    type = typeof(ConstantAstNode);
                    break;
                case "binop":
                    type = typeof(BinaryAstNode);
                    break;
                case "unop":
                    type = typeof(UnaryAstNode);
                    break;
                case "call":
                    type = typeof(MethodCallAstNode);
                    break;
                case "error":
                    type = typeof(ErrorAstNode);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Test error: unrecognized expression type abbreviation '{0}'", exprName));
            }

            Trace.WriteLine(string.Format("{0}: {1}{2} (Current: {3})", indent, new string(' ', indent * 2), type.Name, astNode));

            Assert.That(astNode.GetType(), Is.EqualTo(type));

            if (exprName == "const") {
                Assert.That((astNode as ConstantAstNode).Value, Is.EqualTo(objects[index++]));
            }
            else if (exprName == "binop") {
                Assert.That((astNode as BinaryAstNode).Operator.Kind, Is.EqualTo(objects[index++]));
            }
            else if (exprName == "unop") {
                Assert.That((astNode as UnaryAstNode).Operator.Kind, Is.EqualTo(objects[index++]));
            }
            else if (exprName == "call") {
                Assert.That((astNode as MethodCallAstNode).Token.Kind, Is.EqualTo(objects[index++]));
                Assert.That((astNode as MethodCallAstNode).Token.Value, Is.EqualTo(objects[index++]));
            }

            foreach (var child in astNode.Children) {
                CheckExpression(child, indent + 1, objects, ref index);
            }
        }
    }
}
