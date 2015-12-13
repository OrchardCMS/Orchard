using NUnit.Framework;
using Orchard.Mvc.Routes;

namespace Orchard.Tests.Mvc.Routes {
    [TestFixture]
    public class UrlPrefixTests {
        [Test]
        public void RemoveLeadingSegmentsOnlyMatchesFullSegment() {
            var prefix = new UrlPrefix("foo");
            Assert.That(prefix.RemoveLeadingSegments("~/foo/bar"), Is.EqualTo("~/bar"));
            Assert.That(prefix.RemoveLeadingSegments("~/fooo/bar"), Is.EqualTo("~/fooo/bar"));
            Assert.That(prefix.RemoveLeadingSegments("~/fo/bar"), Is.EqualTo("~/fo/bar"));
        }

        [Test]
        public void RemoveLeadingSegmentsMayContainSlash() {
            var prefix = new UrlPrefix("foo/quux");
            Assert.That(prefix.RemoveLeadingSegments("~/foo/quux/bar"), Is.EqualTo("~/bar"));
            Assert.That(prefix.RemoveLeadingSegments("~/foo/bar"), Is.EqualTo("~/foo/bar"));
            Assert.That(prefix.RemoveLeadingSegments("~/quux/bar"), Is.EqualTo("~/quux/bar"));
        }

        [Test]
        public void RemoveLeadingSegmentsCanMatchEntireUrl() {
            var prefix = new UrlPrefix("foo");
            Assert.That(prefix.RemoveLeadingSegments("~/foo/"), Is.EqualTo("~/"));
            Assert.That(prefix.RemoveLeadingSegments("~/foo"), Is.EqualTo("~/"));
        }

        [Test]
        public void RemoveLeadingSegmentsIsCaseInsensitive() {
            var prefix = new UrlPrefix("Foo");
            Assert.That(prefix.RemoveLeadingSegments("~/foo/bar"), Is.EqualTo("~/bar"));
            Assert.That(prefix.RemoveLeadingSegments("~/FOO/BAR"), Is.EqualTo("~/BAR"));
        }

        [Test]
        public void RemoveLeadingSegmentsIgnoreLeadingAndTrailingCharactersOnInput() {
            var prefix = new UrlPrefix("foo");
            Assert.That(prefix.RemoveLeadingSegments("~/foo/bar"), Is.EqualTo("~/bar"));
            var prefix2 = new UrlPrefix("~/foo");
            Assert.That(prefix2.RemoveLeadingSegments("~/foo/bar"), Is.EqualTo("~/bar"));
            var prefix3 = new UrlPrefix("foo/");
            Assert.That(prefix3.RemoveLeadingSegments("~/foo/bar"), Is.EqualTo("~/bar"));
        }

        [Test]
        public void PrependLeadingSegmentsInsertsBeforeNormalVirtualPath() {
            var prefix = new UrlPrefix("foo");
            Assert.That(prefix.PrependLeadingSegments("~/bar"), Is.EqualTo("~/foo/bar"));
        }

        [Test]
        public void PrependLeadingSegmentsPreservesNatureOfIncomingPath() {
            var prefix = new UrlPrefix("foo");
            Assert.That(prefix.PrependLeadingSegments("~/bar"), Is.EqualTo("~/foo/bar"));
            Assert.That(prefix.PrependLeadingSegments("/bar"), Is.EqualTo("/foo/bar"));
            Assert.That(prefix.PrependLeadingSegments("bar"), Is.EqualTo("foo/bar"));
        }

        [Test]
        public void PrependLeadingSegmentsHandlesShortUrlConditionsAppropriately() {
            var prefix = new UrlPrefix("foo");
            Assert.That(prefix.PrependLeadingSegments("~/"), Is.EqualTo("~/foo/"));
            Assert.That(prefix.PrependLeadingSegments("/"), Is.EqualTo("/foo/"));
            Assert.That(prefix.PrependLeadingSegments("~"), Is.EqualTo("~/foo/"));
            Assert.That(prefix.PrependLeadingSegments(""), Is.EqualTo("foo/"));
        }

    }
}
