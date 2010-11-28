using System.Diagnostics;
using NUnit.Framework;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Tests.Modules.SimpleScripting {
    [TestFixture]
    public class EvaluatorTests {
        [Test]
        public void EvaluateSimpleConstant() {
            var result = EvaluateSimpleExpression("true and true");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateInvalidBooleanExpression() {
            var result = EvaluateSimpleExpression("true and 1");
            Assert.That(result.IsError, Is.True);
            Trace.WriteLine(string.Format("Evaluation error: {0}", result.Error.Message));
        }

        [Test]
        public void EvaluateBooleanExpression() {
            var result = EvaluateSimpleExpression("not true");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.BoolValue, Is.EqualTo(false));
        }

        [Test]
        public void EvaluateSimpleArithmetic() {
            var result = EvaluateSimpleExpression("1 + 2 * 3 - 6 / 2");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(4));
        }

        private EvaluationResult EvaluateSimpleExpression(string expression) {
            var ast = new Parser(expression).Parse();
            var result = new Interpreter().Evalutate(new EvaluationContext {
                Tree = ast,
                MethodInvocationCallback = (m, args) => null
            });
            return result;
        }
    }
}
