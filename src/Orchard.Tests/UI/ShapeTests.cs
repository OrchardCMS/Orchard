using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using ClaySharp;
using NUnit.Framework;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Shapes;
using Orchard.Environment;
using Orchard.Mvc;
using Orchard.UI.Zones;

namespace Orchard.Tests.UI {
    [TestFixture]
    public class ShapeTests : ContainerTestBase {
        private WorkContext _workContext;

        protected override void Register(ContainerBuilder builder) {
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>();
            builder.RegisterType<DefaultWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<PageWorkContext>().As<IWorkContextStateProvider>();
            builder.RegisterType<CoreShapes>().As<IShapeDescriptorBindingStrategy>();
            builder.RegisterType<NumberIsAlwaysFortyTwo>().As<IShapeFactoryEvents>();
        }

        protected override void Resolve(IContainer container) {
            _workContext = container.Resolve<IWorkContextAccessor>().CreateWorkContextScope().WorkContext;
        }

        [Test]
        public void WorkContextPageIsLayoutShape() {
            var page = _workContext.Page;
            ShapeMetadata pageMetadata = page.Metadata;
            Assert.That(pageMetadata.Type, Is.EqualTo("Layout"));
            Assert.That(page.Metadata.Type, Is.EqualTo("Layout"));
        }

        [Test]
        public void PagePropertiesAreNil() {
            var page = _workContext.Page;
            var pageFoo = page.Foo;
            Assert.That(pageFoo == null);
        }

        [Test]
        public void PageZonesPropertyIsNotNil() {
            var page = _workContext.Page;
            var pageZones = page.Zones;
            Assert.That(pageZones != null);
            Assert.That(pageZones.Foo == null);
        }

        [Test]
        public void AddingToZonePropertyMakesItExist() {
            var page = _workContext.Page;
            Assert.That(page.Zones.Foo == null);

            var pageZonesFoo = page.Zones.Foo;
            pageZonesFoo.Add("hello");

            Assert.That(page.Zones.Foo != null);
            Assert.That(page.Foo != null);
            Assert.That(page.Foo.Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test]
        public void AddingToZoneIndexedMakesItExist() {
            var page = _workContext.Page;
            Assert.That(page.Zones["Foo"] == null);

            var pageZonesFoo = page.Zones["Foo"];
            pageZonesFoo.Add("hello");

            Assert.That(page.Zones["Foo"] != null);
            Assert.That(page["Foo"] != null);
            Assert.That(page["Foo"].Metadata.Type, Is.EqualTo("Zone"));
        }


        [Test]
        public void CallingAddOnNilPropertyMakesItBecomeZone() {
            var page = _workContext.Page;
            Assert.That(page.Foo == null);

            page.Foo.Add("hello");

            Assert.That(page.Foo != null);
            Assert.That(page.Foo.Metadata.Type, Is.EqualTo("Zone"));
        }


        [Test]
        public void ZoneContentsAreEnumerable() {
            var page = _workContext.Page;
            Assert.That(page.Foo == null);

            page.Foo.Add("hello");
            page.Foo.Add("world");

            var list = new List<object>();
            foreach (var item in page.Foo) {
                list.Add(item);
            }

            Assert.That(list.Count(), Is.EqualTo(2));
            Assert.That(list.First(), Is.EqualTo("hello"));
            Assert.That(list.Last(), Is.EqualTo("world"));
        }


        class NumberIsAlwaysFortyTwo : ShapeFactoryEvents {
            public override void Creating(ShapeCreatingContext context) {
                context.Behaviors.Add(new Behavior());
            }

            class Behavior : ClayBehavior {
                public override object GetMember(Func<object> proceed, object self, string name) {
                    return name == "Number" ? 42 : proceed();
                }
            }
        }

        [Test]
        public void NumberIsFortyTwo() {
            var page = _workContext.Page;
            Assert.That(page.Number, Is.EqualTo(42));
            Assert.That(page.Foo.Number == null);
            page.Foo.Add("yarg");
            Assert.That(page.Foo.Number, Is.EqualTo(42));
        }
    }

}
