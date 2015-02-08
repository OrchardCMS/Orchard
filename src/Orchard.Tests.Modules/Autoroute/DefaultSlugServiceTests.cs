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
            Assert.That(_slugService.Slugify("«a»-%-.d"), Is.EqualTo("a-d"));
        }

        [Test]
        public void ShouldConvertToLowercase() {
            Assert.That(_slugService.Slugify("ABCDE"), Is.EqualTo("abcde"));
        }

        [Test]
        public void ShouldRemoveDiacritics() {
            Assert.That(_slugService.Slugify("àçéïôù"), Is.EqualTo("aceiou"));
        }

        [Test]
        public void ShouldPreserveCyrilicCharacters() {
            Assert.That(_slugService.Slugify("джинсы_клеш"), Is.EqualTo("джинсы_клеш"));
        }

        [Test]
        public void ShouldPreserveHebrewCharacters() {
            Assert.That(_slugService.Slugify("צוות_אורצ_רד"), Is.EqualTo("צוות_אורצ_רד"));
        }

        [Test]
        public void ShouldPreserveChineseCharacters() {
            Assert.That(_slugService.Slugify("调度模块允许后台任务调度"), Is.EqualTo("调度模块允许后台任务调度"));
        }

        [Test]
        public void ShouldPreserveArabicCharacters() {
            Assert.That(_slugService.Slugify("فريق_الاورشارد"), Is.EqualTo("فريق_الاورشارد"));
        }

        [Test]
        public void ShouldPreserveJapaneseCharacters() {
            Assert.That(_slugService.Slugify("不正なコンテナ"), Is.EqualTo("不正なコンテナ"));
        }
    }
}
