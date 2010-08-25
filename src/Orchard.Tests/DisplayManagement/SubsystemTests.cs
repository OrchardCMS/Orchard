using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Autofac;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Secondary;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class SubsystemTests {
        private IContainer _container;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultShapeBuilder>().As<IShapeBuilder>();
            builder.RegisterType<DisplayHelperFactory>().As<IDisplayHelperFactory>();
            builder.RegisterType<ShapeHelperFactory>().As<IShapeHelperFactory>();
            builder.RegisterType<DefaultShapeTableFactory>().As<IShapeTableFactory>();
            builder.RegisterType<SimpleShapes>().As<IShapeProvider>();
            _container = builder.Build();
        }

        public class SimpleShapes : IShapeProvider {
            public IHtmlString Something() {
                return new HtmlString("<br/>");
            }

            public IHtmlString Pager() {
                return new HtmlString("<div>hello</div>");
            }
        }

        [Test]
        public void RenderingSomething() {
            var viewContext = new ViewContext();
            
            dynamic Display = _container.Resolve<IDisplayHelperFactory>().CreateDisplayHelper(viewContext, null);

            dynamic New = _container.Resolve<IShapeHelperFactory>().CreateShapeHelper();

            var result1 = Display.Something();
            var result2 = ((DisplayHelper)Display).ShapeExecute((Shape)New.Pager());
            
            Display(New.Pager());

            Assert.That(result1.ToString(), Is.EqualTo("<br/>"));
            Assert.That(result2.ToString(), Is.EqualTo("<div>hello</div>"));
        }
    }
}
