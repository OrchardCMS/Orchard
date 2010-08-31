using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Descriptors.ShapeAttributeStrategy;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Tests.Utility;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class SubsystemTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ShapeAttributeBindingModule());
            builder.RegisterType<ShapeAttributeBindingStrategy>().As<IShapeDescriptorBindingStrategy>();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DisplayHelperFactory>().As<IDisplayHelperFactory>();
            builder.RegisterType<ShapeHelperFactory>().As<IShapeHelperFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<SimpleShapes>();
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
            var viewContext = new ViewContext();
            
            dynamic Display = _container.Resolve<IDisplayHelperFactory>().CreateHelper(viewContext, null);

            dynamic New = _container.Resolve<IShapeHelperFactory>().CreateHelper();

            var result1 = Display.Something();
            var result2 = ((DisplayHelper)Display).ShapeExecute((Shape)New.Pager());
            
            Display(New.Pager());

            Assert.That(result1.ToString(), Is.EqualTo("<br/>"));
            Assert.That(result2.ToString(), Is.EqualTo("<div>hello</div>"));
        }
    }
}
