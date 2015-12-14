using System;
using Autofac;
using NUnit.Framework;
using Orchard.Scripting;
using Orchard.Caching;
using Orchard.Tests.Stubs;
using Orchard.Widgets.RuleEngine;
using Orchard.Widgets.Services;

namespace Orchard.Tests.Modules.Widgets {
    [TestFixture]
    public class WidgetsTests {
        private IContainer _container;
        private IRuleManager _ruleManager;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<ScriptExpressionEvaluator>().As<IScriptExpressionEvaluator>();
            builder.RegisterType<AlwaysTrueRuleProvider>().As<IRuleProvider>();
            builder.RegisterType<RuleManager>().As<IRuleManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();

            _container = builder.Build();
            _ruleManager = _container.Resolve<IRuleManager>();
        }

        [Test]
        public void ProviderGetsCalledForExpression() {
            bool result = _ruleManager.Matches("hello");
            Assert.IsTrue(result);
        }

        [Test]
        public void RubyExpressionIsEvaluated() {
            bool result = _ruleManager.Matches("not hello");
            Assert.IsFalse(result);
        }

        [Test]
        public void ArgumentsArePassedCorrectly() {
            bool result = _ruleManager.Matches("add(2, 3) == 5");
            Assert.IsTrue(result);
        }
    }

    public class AlwaysTrueRuleProvider : IRuleProvider {
        public void Process(RuleContext ruleContext) {
            if (ruleContext.FunctionName == "add") {
                ruleContext.Result = Convert.ToInt32(ruleContext.Arguments[0]) + Convert.ToInt32(ruleContext.Arguments[1]);
                return;
            }

            ruleContext.Result = true;
        }
    }
}

