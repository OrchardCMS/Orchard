using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Orchard.Scripting.Compiler;

namespace Orchard.Tests.Modules.Scripting {
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
        public void EvaluateRelationalOperators() {
            var result = EvaluateSimpleExpression("1 < 2");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateRelationalOperators2() {
            var result = EvaluateSimpleExpression("2 <= 2");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateRelationalOperators3() {
            var result = EvaluateSimpleExpression("1 < 2 or 2 > 3 and !false");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateRelationalOperators4() {
            var result = EvaluateSimpleExpression("1 > 2 or 2 > 3 and !false");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(false));
        }

        [Test]
        public void EvaluateRelationalOperators5() {
            var result = EvaluateSimpleExpression("1 > 2 or 4 > 3 and !false");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateRelationalOperators6() {
            var result = EvaluateSimpleExpression("!false");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateEqualityOperators() {
            var result = EvaluateSimpleExpression("1 == 2");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(false));
        }

        [Test]
        public void EvaluateEqualityOperators2() {
            var result = EvaluateSimpleExpression("1 != 2");
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateSimpleMethodCall() {
            var result = EvaluateSimpleExpression("print 1 + 2 * 3 - 6 / 2", 
                (m, args) => (m == "print") ? (int)args[0] * 2 : 0);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(4 * 2));
        }

        [Test]
        public void EvaluateSimpleMethodCall2() {
            var result = EvaluateSimpleExpression("foo 1 + bar 3",
                (m, args) => 
                    (m == "foo") ? (int)args[0] * 2 : 
                    (m == "bar") ? (int)args[0] : 0);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(2 * (1 + 3)));
        }

        [Test]
        public void EvaluateSimpleMethodCall3() {
            var result = EvaluateSimpleExpression("foo(1) + bar(3)",
                (m, args) =>
                    (m == "foo") ? (int)args[0] * 2 :
                    (m == "bar") ? (int)args[0] : 0);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(2 + 3));
        }

        [Test]
        public void EvaluateSimpleMethodCall4() {
            var result = EvaluateSimpleExpression("foo",
                (m, args) => (m == "foo") ? true : false);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        [Test]
        public void EvaluateSimpleMethodCall5() {
            var result = EvaluateSimpleExpression("foo()",
                (m, args) => (m == "foo") ? true : false);
            Assert.That(result.IsError, Is.False);
            Assert.That(result.Value, Is.EqualTo(true));
        }

        private EvaluationResult EvaluateSimpleExpression(string expression) {
            return EvaluateSimpleExpression(expression, (m, args) => null);
        }

        private EvaluationResult EvaluateSimpleExpression(
            string expression, Func<string, IList<object>, object> methodInvocationCallback) {

            var ast = new Parser(expression).Parse();
            foreach(var error in ast.GetErrors()) {
                Trace.WriteLine(string.Format("Error during parsing of '{0}': {1}", expression, error.Message));
            }
            Assert.That(ast.GetErrors().Any(), Is.False);
            var result = new Interpreter().Evalutate(new EvaluationContext {
                Tree = ast,
                MethodInvocationCallback = methodInvocationCallback
            });
            Trace.WriteLine(string.Format("Result of evaluation of '{0}': {1}", expression, result));
            return result;
        }
    }
}
