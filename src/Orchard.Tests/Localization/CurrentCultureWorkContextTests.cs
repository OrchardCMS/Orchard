using Autofac;
using NUnit.Framework;
using Orchard.Localization.Services;
using Orchard.Mvc;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Localization {
    [TestFixture]
    public class CurrentCultureWorkContextTests {
        private IContainer _container;
        private IWorkContextStateProvider _currentCultureStateProvider;
        private WorkContext _workContext;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            _workContext = new StubWorkContext();
            builder.RegisterInstance(new StubCultureSelector("fr-CA")).As<ICultureSelector>();
            builder.RegisterInstance(new StubHttpContext("~/"));
            builder.RegisterInstance(_workContext);
            builder.RegisterType<StubHttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<CurrentCultureWorkContext>().As<IWorkContextStateProvider>();
            _container = builder.Build();
            _currentCultureStateProvider = _container.Resolve<IWorkContextStateProvider>();
        }

        [Test]
        public void CultureManagerReturnsCultureFromSelectors() {
            var actualCulture = _currentCultureStateProvider.Get<string>("CurrentCulture")(_workContext);
            var expectedCulture = "fr-CA";
            Assert.That(actualCulture, Is.EqualTo(expectedCulture));
        }
    }
}