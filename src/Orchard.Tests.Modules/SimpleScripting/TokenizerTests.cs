using NUnit.Framework;
using Orchard.Widgets.SimpleScripting;

namespace Orchard.Tests.Modules.SimpleScriptingTests {
    [TestFixture]
    public class TokenizerTests {

        [Test]
        public void LexerShouldProcessSingleQuotedStringLiteral() {
            TestStringLiteral(@"'toto'", @"toto", TerminalKind.SingleQuotedStringLiteral);
            TestStringLiteral(@"'to\'to'", @"to'to", TerminalKind.SingleQuotedStringLiteral);
            TestStringLiteral(@"'to\\to'", @"to\to", TerminalKind.SingleQuotedStringLiteral);
            TestStringLiteral(@"'to\ato'", @"to\ato", TerminalKind.SingleQuotedStringLiteral);
        }

        [Test]
        public void LexerShouldProcessStringLiteral() {
            TestStringLiteral(@"""toto""", @"toto", TerminalKind.StringLiteral);
            TestStringLiteral(@"""to\'to""", @"to'to", TerminalKind.StringLiteral);
            TestStringLiteral(@"""to\\to""", @"to\to", TerminalKind.StringLiteral);
            TestStringLiteral(@"""to\ato""", @"toato", TerminalKind.StringLiteral);
        }

        private void TestStringLiteral(string value, string expected, TerminalKind expectedTerminalKind) {
            var lexer = new Tokenizer(value);
            var token1 = lexer.NextToken();
            Assert.That(token1.Kind, Is.EqualTo(expectedTerminalKind));
            Assert.That(token1.Value, Is.EqualTo(expected));

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TerminalKind.Eof));
        }

        [Test]
        public void LexerShouldProcessReservedWords() {
            TestReservedWord("true", true, TerminalKind.True);
            TestReservedWord("false", false, TerminalKind.False);
            TestReservedWord("not", null, TerminalKind.Not);
            TestReservedWord("and", null, TerminalKind.And);
            TestReservedWord("or", null, TerminalKind.Or);
        }

        private void TestReservedWord(string expression, object value, TerminalKind expectedTerminalKind) {
            var lexer = new Tokenizer(expression);
            var token1 = lexer.NextToken();
            Assert.That(token1.Kind, Is.EqualTo(expectedTerminalKind));
            Assert.That(token1.Value, Is.EqualTo(value));

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TerminalKind.Eof));
        }

        [Test]
        public void LexerShouldProcesSequenceOfTokens() {
            CheckTokenSequence("true false", TerminalKind.True, TerminalKind.False);
            CheckTokenSequence("true toto false", TerminalKind.True, TerminalKind.Identifier, TerminalKind.False);
        }

        [Test]
        public void LexerShouldProcesSequenceOfTokens2() {
            CheckTokenSequence("1+2*3", TerminalKind.Integer, TerminalKind.Plus, TerminalKind.Integer, TerminalKind.Mul, TerminalKind.Integer);
        }


        private void CheckTokenSequence(string expression, params TerminalKind[] terminalKinds) {
            var lexer = new Tokenizer(expression);
            foreach (var kind in terminalKinds) {
                var token = lexer.NextToken();
                Assert.That(token.Kind, Is.EqualTo(kind));
            }

            var token2 = lexer.NextToken();
            Assert.That(token2.Kind, Is.EqualTo(TerminalKind.Eof));
        }
    }
}
