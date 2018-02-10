using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.Core.Shapes;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.Tests.DisplayManagement;
using Orchard.UI.Zones;

namespace Orchard.Tests.UI {
    [TestFixture]
    public class ShapeTests : ContainerTestBase {
        dynamic _layout;

        protected override void Register(ContainerBuilder builder) {
            var defaultShapeTable = new ShapeTable {
                Descriptors = new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
                Bindings = new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
            };
            builder.Register(ctx => defaultShapeTable);

            builder.RegisterType<DefaultDisplayManagerTests.TestWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<LayoutWorkContext>().As<IWorkContextStateProvider>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultDisplayManager>().As<IDisplayManager>();
            builder.RegisterType<DefaultDisplayManagerTests.TestShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<CoreShapes>().As<IShapeTableProvider>();
        }

        protected override void Resolve(ILifetimeScope container) {
            var shapeFactory = _container.Resolve<IShapeFactory>();
            _layout = new ZoneHolding(() => shapeFactory.Create("Zone"));
        }

        [Test]
        public void PagePropertiesAreNil() {
            
            var pageFoo = _layout.Foo;
            Assert.That(pageFoo == null);
        }

        [Test]
        public void PageZonesPropertyIsNotNil() {
            var pageZones = _layout.Zones;
            Assert.That(pageZones != null);
            Assert.That(pageZones.Foo == null);
        }

        [Test]
        public void AddingToZonePropertyMakesItExist() {
            Assert.That(_layout.Zones.Foo == null);

            var pageZonesFoo = _layout.Zones.Foo;
            pageZonesFoo.Add("hello");

            Assert.That(_layout.Zones.Foo != null);
            Assert.That(_layout.Foo != null);
            Assert.That(_layout.Foo.Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test]
        public void AddingToZoneIndexedMakesItExist() {
            Assert.That(_layout.Zones["Foo"] == null);

            var pageZonesFoo = _layout.Zones["Foo"];
            pageZonesFoo.Add("hello");

            Assert.That(_layout.Zones["Foo"] != null);
            Assert.That(_layout["Foo"] != null);
            Assert.That(_layout["Foo"].Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test]
        public void CallingAddOnNilPropertyMakesItBecomeZone() {
            Assert.That(_layout.Foo == null);

            _layout.Foo.Add("hello");

            Assert.That(_layout.Foo != null);
            Assert.That(_layout.Foo.Metadata.Type, Is.EqualTo("Zone"));
        }

        [Test]
        public void ZoneContentsAreEnumerable() {
            Assert.That(_layout.Foo == null);

            _layout.Foo.Add("hello");
            _layout.Foo.Add("world");

            var list = new List<object>();
            foreach (var item in _layout.Foo) {
                list.Add(item);
            }

            Assert.That(list.Count(), Is.EqualTo(2));
            Assert.That(list.First(), Is.EqualTo("hello"));
            Assert.That(list.Last(), Is.EqualTo("world"));
        }

        [Test]
        public void ZoneContentsCastBeConvertedToEnunerableOfObject() {
            Assert.That(_layout.Foo == null);

            _layout.Foo.Add("hello");
            _layout.Foo.Add("world");

            IEnumerable<object> list = _layout.Foo;

            Assert.That(list.Count(), Is.EqualTo(2));
            Assert.That(list.First(), Is.EqualTo("hello"));
            Assert.That(list.Last(), Is.EqualTo("world"));

            var first = ((IEnumerable<object>)_layout.Foo).FirstOrDefault();
            Assert.That(first, Is.EqualTo("hello"));
        }

        [Test]
        public void ZoneContentsCastBeConvertedToEnunerableOfDynamics() {
            Assert.That(_layout.Foo == null);

            _layout.Foo.Add("hello");
            _layout.Foo.Add("world");

            IEnumerable<dynamic> list = _layout.Foo;

            Assert.That(list.Count(), Is.EqualTo(2));
            Assert.That(list.First(), Is.EqualTo("hello"));
            Assert.That(list.Last(), Is.EqualTo("world"));

            var first = ((IEnumerable<dynamic>) _layout.Foo).FirstOrDefault();
            Assert.That(first, Is.EqualTo("hello"));
        }
    }
}
