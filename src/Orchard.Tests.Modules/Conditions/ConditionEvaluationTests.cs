using System;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Conditions.Services;
using Orchard.Scripting;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Conditions {
    [TestFixture]
    public class ConditionEvaluationTests {
        private IContainer _container;
        private IConditionManager _conditionManager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ScriptExpressionEvaluator>().As<IScriptExpressionEvaluator>();
            builder.RegisterType<AlwaysTrueCondition>().As<IConditionProvider>();
            builder.RegisterType<ConditionManager>().As<IConditionManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();

            _container = builder.Build();
            _conditionManager = _container.Resolve<IConditionManager>();
        }

        [Test]
        public void ProviderGetsCalledForExpression() {
            var result = _conditionManager.Matches("hello");
            Assert.IsTrue(result);
        }

        [Test]
        public void RubyExpressionIsEvaluated() {
            var result = _conditionManager.Matches("not hello");
            Assert.IsFalse(result);
        }

        [Test]
        public void ArgumentsArePassedCorrectly() {
            var result = _conditionManager.Matches("add(2, 3) == 5");
            Assert.IsTrue(result);
        }
    }

    public class AlwaysTrueCondition : IConditionProvider {
        public void Evaluate(ConditionEvaluationContext evaluationContext) {
            if (evaluationContext.FunctionName == "add") {
                evaluationContext.Result = Convert.ToInt32(evaluationContext.Arguments[0]) + Convert.ToInt32(evaluationContext.Arguments[1]);
                return;
            }

            evaluationContext.Result = true;
        }
    }
}

