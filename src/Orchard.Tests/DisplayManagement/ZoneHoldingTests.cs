using System;
using NUnit.Framework;
using Orchard.DisplayManagement.Shapes;
using Orchard.UI.Zones;

namespace Orchard.Tests.DisplayManagement {
    [TestFixture]
    public class ZoneHoldingTests {

        [Test]
        public void ZonesShouldReturn() {
            Func<dynamic> factory = () => new Shape();

            var foo = new ZoneHolding(factory);
            Assert.That(foo.Zones, Is.InstanceOf<Zones>());
        }

        [Test]
        public void MemberShouldCreateAZone() {
            Func<dynamic> factory = () => new Shape();

            dynamic foo = new ZoneHolding(factory);
            Assert.That(foo.Header, Is.InstanceOf<ZoneOnDemand>());
        }

        [Test]
        public void IndexShouldCreateAZone() {
            Func<dynamic> factory = () => new Shape();

            dynamic foo = new ZoneHolding(factory);
            Assert.That(foo.Zones["Header"], Is.InstanceOf<ZoneOnDemand>());
        }

        [Test]
        public void ZonesMemberShouldCreateAZone() {
            Func<dynamic> factory = () => new Shape();

            dynamic foo = new ZoneHolding(factory);
            Assert.That(foo.Zones.Header, Is.InstanceOf<ZoneOnDemand>());
        }

        [Test]
        public void ZonesShouldBeUnique() {
            Func<dynamic> factory = () => new Shape();

            dynamic foo = new ZoneHolding(factory);
            var header = foo.Header;

            Assert.That(foo.Zones.Header, Is.EqualTo(header));
            Assert.That(foo.Zones["Header"], Is.EqualTo(header));
            Assert.That(foo.Header, Is.EqualTo(header));
        }


        [Test]
        public void EmptyZonesShouldBeNull() {
            Func<dynamic> factory = () => new Shape();

            dynamic foo = new ZoneHolding(factory);

            Assert.That(foo.Header == 1, Is.False);
            Assert.That(foo.Header != 1, Is.True);

            dynamic header = foo.Header;

            Assert.That(header == null, Is.True);
            Assert.That(header != null, Is.False);

            Assert.That(header == Nil.Instance, Is.True);
            Assert.That(header != Nil.Instance, Is.False);
        }

        [Test]
        public void NoneEmptyZonesShouldNotBeNull() {
            Func<dynamic> factory = () => new Shape();

            dynamic foo = new ZoneHolding(factory);

            Assert.That(foo.Header == null, Is.True);
            Assert.That(foo.Header != null, Is.False);

            foo.Header.Add("blah");

            Assert.That(foo.Header == null, Is.False);
            Assert.That(foo.Header != null, Is.True);

            Assert.That(foo.Header == Nil.Instance, Is.False);
            Assert.That(foo.Header != Nil.Instance, Is.True);
        }

    }
}
