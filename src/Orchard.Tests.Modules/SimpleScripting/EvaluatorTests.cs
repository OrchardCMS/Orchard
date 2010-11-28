using System;
using NUnit.Framework;
using Orchard.Widgets.SimpleScripting;
using Orchard.Widgets.SimpleScripting.Compiler;

namespace Orchard.Tests.Modules.SimpleScriptingTests {
    [TestFixture]
    public class EvaluatorTests {
        [Test]
        public void EvaluateSimpleConstant() {
            var result = EvaluateSimpleExpression("true and true");
            Assert.That(result.HasErrors, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateSimpleArithmetic() {
            var result = EvaluateSimpleExpression("1 + 2 * 3 - 6 / 2");
            Assert.That(result.HasErrors, Is.False);
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
