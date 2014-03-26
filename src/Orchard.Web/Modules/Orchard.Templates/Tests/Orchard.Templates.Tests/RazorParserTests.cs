using System.Web;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.Templates.Compilation.Razor;
using Orchard.Templates.Services;
using Orchard.Tests.Stubs;

namespace Orchard.Templates.Tests {
    [TestFixture]
    public class RazorParserTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<RazorTemplateProcessor>().As<ITemplateProcessor>();
            builder.RegisterType<RazorCompiler>().As<IRazorCompiler>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<StubHttpContext>().As<HttpContextBase>();

            _container = builder.Build();
        }

        [Test]
        public void ParseSomething() {
            var razorParser = _container.Resolve<ITemplateProcessor>();
            var result = razorParser.Process("@{ int i = 42;} The answer to everything is @i.", "Foo");
            Assert.That(result.Trim(), Is.EqualTo("The answer to everything is 42."));
        }
    }
}
