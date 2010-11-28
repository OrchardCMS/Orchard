using System;
using System.Collections.Generic;
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

        [Test]
        public void EvaluateSimpleMethodCall() {
            var result = EvaluateSimpleExpression("print 1 + 2 * 3 - 6 / 2", 
                (m, args) => (m == "print") ? (int)args[0] * 2 : 0);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(4 * 2));
        }

        private EvaluationResult EvaluateSimpleExpression(string expression) {
            return EvaluateSimpleExpression(expression, (m, args) => null);
        }

        private EvaluationResult EvaluateSimpleExpression(
            string expression, Func<string, IList<object>, object> methodInvocationCallback) {

            var ast = new Parser(expression).Parse();
            var result = new Interpreter().Evalutate(new EvaluationContext {
                Tree = ast,
                MethodInvocationCallback = methodInvocationCallback
            });
            return result;
        }
    }
}
