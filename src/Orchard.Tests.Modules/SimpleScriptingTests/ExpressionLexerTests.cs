using NUnit.Framework;
using Orchard.Widgets.SimpleScripting;

namespace Orchard.Tests.Modules.SimpleScriptingTests {
    [TestFixture]
    public class ExpressionLexerTests {

        [Test]
        public void LexerShouldProcessSingleQuotedStringLiteral() {
            TestStringLiteral(@"'toto'", @"toto", TokenKind.SingleQuotedStringLiteral);
            TestStringLiteral(@"'to\'to'", @"to'to", TokenKind.SingleQuotedStringLiteral);
            TestStringLiteral(@"'to\\to'", @"to\to", TokenKind.SingleQuotedStringLiteral);
            TestStringLiteral(@"'to\ato'", @"to\ato", TokenKind.SingleQuotedStringLiteral);
        }

        [Test]
        public void LexerShouldProcessStringLiteral() {
            TestStringLiteral(@"""toto""", @"toto", TokenKind.StringLiteral);
            TestStringLiteral(@"""to\'to""", @"to'to", TokenKind.StringLiteral);
            TestStringLiteral(@"""to\\to""", @"to\to", TokenKind.StringLiteral);
            TestStringLiteral(@"""to\ato""", @"toato", TokenKind.StringLiteral);
        }

        private void TestStringLiteral(string value, string expected, TokenKind expectedTokenKind) {
            var lexer = new ExpressionTokenizer(value);
            var token1 = lexer.NextToken();
            Assert.That(token1.Kind, Is.EqualTo(expectedTokenKind));
            Assert.That(token1.Value, Is.EqualTo(expected));

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TokenKind.Eof));
        }

        [Test]
        public void LexerShouldProcessReservedWords() {
            TestReservedWord("true", true, TokenKind.True);
            TestReservedWord("false", false, TokenKind.False);
            TestReservedWord("not", "not", TokenKind.Not);
            TestReservedWord("and", "and", TokenKind.And);
            TestReservedWord("or", "or", TokenKind.Or);
        }

        private void TestReservedWord(string expression, object value, TokenKind expectedTokenKind) {
            var lexer = new ExpressionTokenizer(expression);
            var token1 = lexer.NextToken();
            Assert.That(token1.Kind, Is.EqualTo(expectedTokenKind));
            Assert.That(token1.Value, Is.EqualTo(value));

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TokenKind.Eof));
        }

        [Test]
        public void LexerShouldProcesSequenceOfTokens() {
            CheckTokenSequence("true false", TokenKind.True, TokenKind.False);
            CheckTokenSequence("true toto false", TokenKind.True, TokenKind.Identifier, TokenKind.False);
        }

        [Test]
        public void LexerShouldProcesSequenceOfTokens2() {
            CheckTokenSequence("1+2*3", TokenKind.NumberLiteral, TokenKind.Plus, TokenKind.NumberLiteral, TokenKind.Mul, TokenKind.NumberLiteral);
        }


        private void CheckTokenSequence(string expression, params TokenKind[] tokenKinds) {
            var lexer = new ExpressionTokenizer(expression);
            foreach (var kind in tokenKinds) {
                var token = lexer.NextToken();
                Assert.That(token.Kind, Is.EqualTo(kind));
            }

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TokenKind.Eof));
        }
    }
}
