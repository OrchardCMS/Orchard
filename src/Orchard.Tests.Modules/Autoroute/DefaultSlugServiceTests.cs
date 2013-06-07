using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Orchard.Autoroute.Services;

namespace Orchard.Tests.Modules.Autoroute {
    [TestFixture]
    public class DefaultSlugServiceTests {
        [Test]
        public void ShouldStripContiguousDashes() {
            DefaultSlugService slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

            Assert.That(slugService.Slugify("a - b"), Is.EqualTo("a-b"));
        }

        [Test]
        public void ShouldStripContiguousDashes2() {
            DefaultSlugService slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

            Assert.That(slugService.Slugify("a  -  -      -  -   -   -b"), Is.EqualTo("a-b"));
        }

        [Test]
        public void ShouldStripContiguousDashesEverywhere() {
            DefaultSlugService slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

            Assert.That(slugService.Slugify("a  -  b - c -- d"), Is.EqualTo("a-b-c-d"));
        }

        [Test]
        public void ShouldChangePercentSymbolsToHyphans() {
            DefaultSlugService slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

            Assert.That(slugService.Slugify("a%d"), Is.EqualTo("a-d"));
        }

        [Test]
        public void ShouldChangeDotSymbolsToHyphans() {
            DefaultSlugService slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

            Assert.That(slugService.Slugify("a,d"), Is.EqualTo("a-d"));
        }

        [Test]
        public void ShouldMakeSureFunkycharactersAndHyphansOnlyReturnSingleHyphan() {
            DefaultSlugService slugService = new DefaultSlugService(new Mock<ISlugEventHandler>().Object);

            Assert.That(slugService.Slugify("a-%-.d"), Is.EqualTo("a-d"));
        }
    }
}
