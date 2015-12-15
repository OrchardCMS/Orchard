using NUnit.Framework;
using Orchard.Scripting.Compiler;

namespace Orchard.Tests.Modules.Scripting {
    [TestFixture]
    public class TokenizerTests {

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
            var lexer = new Tokenizer(value);
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
            TestReservedWord("nil", null, TokenKind.NullLiteral);
            TestReservedWord("null", null, TokenKind.NullLiteral);
            TestReservedWord("not", null, TokenKind.Not);
            TestReservedWord("and", null, TokenKind.And);
            TestReservedWord("or", null, TokenKind.Or);
        }

        private void TestReservedWord(string expression, object value, TokenKind expectedTokenKind) {
            var lexer = new Tokenizer(expression);
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
            CheckTokenSequence("1+2*3", TokenKind.Integer, TokenKind.Plus, TokenKind.Integer, TokenKind.Mul, TokenKind.Integer);
        }

        [Test]
        public void LexerShouldProcesSequenceOfTokens3() {
            CheckTokenSequence("= == < <= > >= ! !=", TokenKind.Equal, TokenKind.EqualEqual, 
                TokenKind.LessThan, TokenKind.LessThanEqual, 
                TokenKind.GreaterThan, TokenKind.GreaterThanEqual, TokenKind.NotSign, TokenKind.NotEqual);
        }

        private void CheckTokenSequence(string expression, params TokenKind[] tokenKinds) {
            var lexer = new Tokenizer(expression);
            foreach (var kind in tokenKinds) {
                var token = lexer.NextToken();
                Assert.That(token.Kind, Is.EqualTo(kind));
            }

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TokenKind.Eof));
        }
    }
}
