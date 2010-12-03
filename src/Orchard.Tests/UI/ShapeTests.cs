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
            builder.RegisterType<WorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<LayoutWorkContext>().As<IWorkContextStateProvider>();
            //builder.RegisterType<CoreShapes>().As<IShapeTableProvider>();
            builder.RegisterType<NumberIsAlwaysFortyTwo>().As<IShapeFactoryEvents>();

            throw new NotImplementedException("this test fixture needs to move to modules tests now");
        }

        protected override void Resolve(ILifetimeScope container) {
            _workContext = container.Resolve<IWorkContextAccessor>().CreateWorkContextScope().WorkContext;
        }

        [Test, Ignore("implementation pending")]
        public void WorkContextPageIsLayoutShape() {
            var layout = _workContext.Layout;
            ShapeMetadata pageMetadata = layout.Metadata;
            Assert.That(pageMetadata.Type, Is.EqualTo("Layout"));
            Assert.That(layout.Metadata.Type, Is.EqualTo("Layout"));
        }

        [Test, Ignore("implementation pending")]
        public void PagePropertiesAreNil() {
            var layout = _workContext.Layout;
            var pageFoo = layout.Foo;
            Assert.That(pageFoo == null);
        }

        [Test, Ignore("implementation pending")]
        public void PageZonesPropertyIsNotNil() {
            var layout = _workContext.Layout;
            var pageZones = layout.Zones;
            Assert.That(pageZones != null);
            Assert.That(pageZones.Foo == null);
        }

        [Test, Ignore("implementation pending")]
        public void AddingToZonePropertyMakesItExist() {
            var layout = _workContext.Layout;
            Assert.That(layout.Zones.Foo == null);

            var pageZonesFoo = layout.Zones.Foo;
            pageZonesFoo.Add("hello");

            Assert.That(layout.Zones.Foo != null);
            Assert.That(layout.Foo != null);
            Assert.That(layout.Foo.Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test, Ignore("implementation pending")]
        public void AddingToZoneIndexedMakesItExist() {
            var layout = _workContext.Layout;
            Assert.That(layout.Zones["Foo"] == null);

            var pageZonesFoo = layout.Zones["Foo"];
            pageZonesFoo.Add("hello");

            Assert.That(layout.Zones["Foo"] != null);
            Assert.That(layout["Foo"] != null);
            Assert.That(layout["Foo"].Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test, Ignore("implementation pending")]
        public void CallingAddOnNilPropertyMakesItBecomeZone() {
            var layout = _workContext.Layout;
            Assert.That(layout.Foo == null);

            layout.Foo.Add("hello");

            Assert.That(layout.Foo != null);
            Assert.That(layout.Foo.Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test, Ignore("implementation pending")]
        public void ZoneContentsAreEnumerable() {
            var layout = _workContext.Layout;
            Assert.That(layout.Foo == null);

            layout.Foo.Add("hello");
            layout.Foo.Add("world");

            var list = new List<object>();
            foreach (var item in layout.Foo) {
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

        [Test, Ignore("implementation pending")]
        public void NumberIsFortyTwo() {
            var layout = _workContext.Layout;
            Assert.That(layout.Number, Is.EqualTo(42));
            Assert.That(layout.Foo.Number == null);
            layout.Foo.Add("yarg");
            Assert.That(layout.Foo.Number, Is.EqualTo(42));
        }
    }

}
