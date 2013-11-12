using NUnit.Framework;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Tests.DisplayManagement.Descriptors {
    [TestFixture]
    public class LocationParserTests : ContainerTestBase {

        [Test]
        public void ZoneShouldBeParsed() {
            Assert.That(new PlacementInfo { Location = "/Content" }.GetZone(), Is.EqualTo("Content"));
            Assert.That(new PlacementInfo { Location = "Content" }.GetZone(), Is.EqualTo("Content"));
            Assert.That(new PlacementInfo { Location = "Content:5" }.GetZone(), Is.EqualTo("Content"));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1" }.GetZone(), Is.EqualTo("Content"));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1" }.GetZone(), Is.EqualTo("Content"));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetZone(), Is.EqualTo("Content"));
        }

        [Test]
        public void PositionShouldBeParsed() {
            Assert.That(new PlacementInfo { Location = "Content" }.GetPosition(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5" }.GetPosition(), Is.EqualTo("5"));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1" }.GetPosition(), Is.EqualTo("5"));
            Assert.That(new PlacementInfo { Location = "Content:5.1#Tab1" }.GetPosition(), Is.EqualTo("5.1"));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1" }.GetPosition(), Is.EqualTo("5"));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetPosition(), Is.EqualTo("5"));
        }

        [Test]
        public void LayoutZoneShouldBeParsed() {
            Assert.That(new PlacementInfo { Location = "/Content" }.IsLayoutZone(), Is.EqualTo(true));
            Assert.That(new PlacementInfo { Location = "/Content:5" }.IsLayoutZone(), Is.EqualTo(true));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1" }.IsLayoutZone(), Is.EqualTo(false));
            Assert.That(new PlacementInfo { Location = "Content:5.1#Tab1" }.IsLayoutZone(), Is.EqualTo(false));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1" }.IsLayoutZone(), Is.EqualTo(false));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1#Tab1" }.IsLayoutZone(), Is.EqualTo(false));
        }

        [Test]
        public void TabShouldBeParsed() {
            Assert.That(new PlacementInfo { Location = "Content" }.GetTab(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5" }.GetTab(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1" }.GetTab(), Is.EqualTo("Tab1"));
            Assert.That(new PlacementInfo { Location = "Content:5.1#Tab1" }.GetTab(), Is.EqualTo("Tab1"));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1" }.GetTab(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetTab(), Is.EqualTo("Tab1"));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1@Group1" }.GetTab(), Is.EqualTo("Tab1"));
        }

        [Test]
        public void GroupShouldBeParsed() {
            Assert.That(new PlacementInfo { Location = "Content" }.GetGroup(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5" }.GetGroup(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1" }.GetGroup(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5.1#Tab1" }.GetGroup(), Is.EqualTo(""));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1" }.GetGroup(), Is.EqualTo("Group1"));
            Assert.That(new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetGroup(), Is.EqualTo("Group1"));
            Assert.That(new PlacementInfo { Location = "Content:5#Tab1@Group1" }.GetGroup(), Is.EqualTo("Group1"));
        }
    }
}
