using System;
using Autofac;
using NUnit.Framework;
using Orchard.Environment.Configuration;
using Orchard.Mvc;
using Orchard.Tests.Stubs;
using Orchard.Widgets.RuleEngine;
using Orchard.Widgets.Services;

namespace Orchard.Tests.Modules.Widgets.RuleEngine {
    [TestFixture]
    public class UrlRuleProviderTest {
        private IContainer _container;
        private IRuleProvider _urlRuleProvider;
        private StubHttpContextAccessor _stubContextAccessor;
        private ShellSettings _shellSettings;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _shellSettings = new ShellSettings { RequestUrlPrefix = String.Empty };
            builder.RegisterType<UrlRuleProvider>().As<IRuleProvider>();
            builder.RegisterInstance(_shellSettings);
            _stubContextAccessor = new StubHttpContextAccessor();
            builder.RegisterInstance(_stubContextAccessor).As<IHttpContextAccessor>();
            _container = builder.Build();
            _urlRuleProvider = _container.Resolve<IRuleProvider>();
        }

        [Test]
        public void UrlForHomePageMatchesHomePagePath() {
            _stubContextAccessor.Set(new StubHttpContext("~/"));
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.True);
        }

        [Test]
        public void UrlForAboutPageMatchesAboutPagePath() {
            _stubContextAccessor.Set(new StubHttpContext("~/about"));
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/about" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.True);
        }

        [Test]
        public void UrlForBlogWithEndingWildcardMatchesBlogPostPageInSaidBlog() {
            _stubContextAccessor.Set(new StubHttpContext("~/my-blog/my-blog-post"));
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/my-blog/*" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.True);
        }

        [Test]
        public void UrlForHomePageDoesNotMatchAboutPagePath() {
            _stubContextAccessor.Set(new StubHttpContext("~/about"));
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.False);
        }

        [Test]
        public void UrlForAboutPageMatchesDifferentCasedAboutPagePath() {
            _stubContextAccessor.Set(new StubHttpContext("~/About"));
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/about" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.True);
        }

        [Test]
        public void UrlForAboutPageWithEndingSlashMatchesAboutPagePath() {
            _stubContextAccessor.Set(new StubHttpContext("~/About/"));
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/about" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.True);
        }

        [Test]
        public void UrlForHomePageMatchesHomePagePathWithUrlPrefix() {
            _stubContextAccessor.Set(new StubHttpContext("~/site1"));
            _shellSettings.RequestUrlPrefix = "site1";
            var context = new RuleContext { FunctionName = "url", Arguments = new[] { "~/" } };
            _urlRuleProvider.Process(context);
            Assert.That(context.Result, Is.True);
        }
    }
}