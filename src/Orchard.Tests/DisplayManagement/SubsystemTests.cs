using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;
using Orchard.Tests.Utility;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class SubsystemTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var testFeature = new Feature
            {
                Descriptor = new FeatureDescriptor
                {
                    Name = "Testing",
                    Extension = new ExtensionDescriptor
                    {
                        Name = "Testing",
                        ExtensionType = "Module",
                    }
                }
            };

            var workContext = new DefaultDisplayManagerTests.TestWorkContext
            {
                CurrentTheme = new DefaultDisplayManagerTests.Theme { ThemeName = "Hello" }
            };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ShapeAttributeBindingModule());
            builder.RegisterType<ShapeAttributeBindingStrategy>().As<IShapeTableProvider>();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DisplayHelperFactory>().As<IDisplayHelperFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterInstance(new DefaultDisplayManagerTests.TestWorkContextAccessor(workContext)).As<IWorkContextAccessor>();
            builder.RegisterInstance(new SimpleShapes()).WithMetadata("Feature", testFeature);
            builder.RegisterInstance(new RouteCollection());
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

            [Shape]
            public IHtmlString Pager() {
                return new HtmlString("<div>hello</div>");
            }
        }

        [Test]
        public void RenderingSomething() {
            dynamic displayHelperFactory = _container.Resolve<IDisplayHelperFactory>().CreateHelper(new ViewContext(), null);
            dynamic shapeHelperFactory = _container.Resolve<IShapeFactory>();

            var result1 = displayHelperFactory.Something();
            var result2 = ((DisplayHelper)displayHelperFactory).ShapeExecute((Shape)shapeHelperFactory.Pager());

            displayHelperFactory(shapeHelperFactory.Pager());

            Assert.That(result1.ToString(), Is.EqualTo("<br/>"));
            Assert.That(result2.ToString(), Is.EqualTo("<div>hello</div>"));
        }
    }
}