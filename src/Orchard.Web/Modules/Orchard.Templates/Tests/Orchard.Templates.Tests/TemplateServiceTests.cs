using System.Web;
using System.Web.Routing;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Templates.Services;
using Orchard.Tests.DisplayManagement;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace Orchard.Templates.Tests {
    [TestFixture]
    public class TemplateServiceTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var workContext = new DefaultDisplayManagerTests.TestWorkContext {
                CurrentTheme = new ExtensionDescriptor { Id = "Hello" }
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ShapeAttributeBindingModule());
            builder.RegisterType<ShapeAttributeBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DisplayHelperFactory>().As<IDisplayHelperFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();
            builder.RegisterInstance(new DefaultDisplayManagerTests.TestWorkContextAccessor(workContext)).As<IWorkContextAccessor>();
            builder.RegisterInstance(new SimpleShapes());
            builder.RegisterInstance(new RouteCollection());
            builder.RegisterType<DefaultTemplateService>().As<ITemplateService>();
            builder.RegisterAutoMocking(MockBehavior.Loose);

            _container = builder.Build();
            _container.Resolve<Mock<IOrchardHostContainer>>()
                .Setup(x => x.Resolve<IComponentContext>())
                .Returns(_container);
        }

        public class SimpleShapes {
            [Shape]
            public IHtmlString Something() {
                return new HtmlString("<br/>");
            }
        }

        [Test]
        public void RenderSomething() {
            var templateService = _container.Resolve<ITemplateService>();
            var result = templateService.ExecuteShape("Something");
            Assert.That(result, Is.EqualTo("<br/>"));
        }
    }
}
