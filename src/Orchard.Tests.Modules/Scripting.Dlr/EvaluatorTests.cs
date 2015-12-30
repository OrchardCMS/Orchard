using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;
using NUnit.Framework;
using Orchard.Scripting;
using Orchard.Scripting.Compiler;
using Orchard.Scripting.Dlr.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Scripting.Dlr {
    [TestFixture]
    public class EvaluatorTests : EvaluatorTestsBase {
        private IContainer _container;
        private IScriptingManager _scriptingManager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<RubyScriptingRuntime>().As<IScriptingRuntime>();
            builder.RegisterType<ScriptingManager>().As<IScriptingManager>();
            _container = builder.Build();
            _scriptingManager = _container.Resolve<IScriptingManager>();
        }

        protected override EvaluationResult EvaluateSimpleExpression(string expression, Func<string, IList<object>, object> methodInvocationCallback) {
            var evaluator = new RubyScriptExpressionEvaluator(_scriptingManager, new StubCacheManager());
            try {
                var value = evaluator.Evaluate(expression, new[] { new GlobalMethodProvider(methodInvocationCallback) });
                return new EvaluationResult(value);
            }
            catch (Exception e) {
                Trace.WriteLine(string.Format("Error during evaluation of '{0}': {1}", expression, e.Message));
                return new EvaluationResult(new Error { Message = e.Message, Exception = e });
            }
        }

        private class GlobalMethodProvider : IGlobalMethodProvider {
            private readonly Func<string, IList<object>, object> _methodInvocationCallback;

            public GlobalMethodProvider(Func<string, IList<object>, object> methodInvocationCallback) {
                _methodInvocationCallback = methodInvocationCallback;
            }

            public void Process(GlobalMethodContext context) {
                context.Result = _methodInvocationCallback(context.FunctionName, context.Arguments);
            }
        }
    }
}
