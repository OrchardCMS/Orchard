using Moq;
using NUnit.Framework;
using Orchard.Autoroute.Services;

namespace Orchard.Tests.Modules.Autoroute {
    [TestFixture]
    public class DefaultSlugServiceTests {

        private DefaultSlugService _slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

        [Test]
        public void ShouldStripContiguousDashes() {
            Assert.That(_slugService.Slugify("a - b"), Is.EqualTo("a-b"));
        }

        [Test]
        public void ShouldStripContiguousDashes2() {
            Assert.That(_slugService.Slugify("a  -  -      -  -   -   -b"), Is.EqualTo("a-b"));
        }

        [Test]
        public void ShouldStripContiguousDashesEverywhere() {
            Assert.That(_slugService.Slugify("a  -  b - c -- d"), Is.EqualTo("a-b-c-d"));
        }

        [Test]
        public void ShouldChangePercentSymbolsToHyphans() {
            Assert.That(_slugService.Slugify("a%d"), Is.EqualTo("a-d"));
        }

        [Test]
        public void ShouldChangeDotSymbolsToHyphans() {
            Assert.That(_slugService.Slugify("a,d"), Is.EqualTo("a-d"));
        }

        [Test]
        public void ShouldMakeSureFunkycharactersAndHyphansOnlyReturnSingleHyphan() {
            Assert.That(_slugService.Slugify("a-%-.d"), Is.EqualTo("a-d"));
        }

        [Test]
        public void ShouldPreserveCyrilicCharacters() {
            Assert.That(_slugService.Slugify("джинсы_клеш"), Is.EqualTo("джинсы_клеш"));
        }
    }
}
