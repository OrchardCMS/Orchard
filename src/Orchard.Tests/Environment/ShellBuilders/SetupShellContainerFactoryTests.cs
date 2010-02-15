using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using NUnit.Framework;
using Orchard.Environment;
using Orchard.Environment.ShellBuilders;
using Orchard.Mvc.Filters;
using Orchard.Mvc.ViewEngines;

namespace Orchard.Tests.Environment.ShellBuilders {
    [TestFixture]
    public class SetupShellContainerFactoryTests {
        private IContainer _hostContainer;

        [SetUp]
        public void Init() {
            _hostContainer = OrchardStarter.CreateHostContainer(builder => {
                builder.Register(new ViewEngineCollection());
                builder.Register(new RouteCollection());
                builder.Register(new ModelBinderDictionary());
            });
        }

        [Test, Ignore("Can't be made to work until module settings and infrastructres implemented")]
        public void FactoryShouldCreateContainerThatProvidesShell() {

            var factory = new SafeModeShellContainerFactory(_hostContainer);
            var shellContainer = factory.CreateContainer(null);
            Assert.That(shellContainer, Is.Not.Null);
            var shell = shellContainer.Resolve<IOrchardShell>();
            Assert.That(shell, Is.Not.Null);
        }

        [Test, Ignore("Can't be made to work until module settings and infrastructres implemented")]
        public void ShellContainerShouldProvideLayoutViewEngine() {
            var factory = new SafeModeShellContainerFactory(_hostContainer);
            var shellContainer = factory.CreateContainer(null);
            var viewEngineFilter = shellContainer.Resolve<IEnumerable<IFilterProvider>>()
                .Single(f=>f is ViewEngineFilter);
            Assert.That(viewEngineFilter, Is.Not.Null);
        }
    }
}
