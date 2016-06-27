using NUnit.Framework;
using Orchard.Core.Common.Services;

namespace Orchard.Core.Tests.Common {
    [TestFixture]
    public class BbcodeFilterTests {

        private readonly BbcodeFilter _filter = new BbcodeFilter();

        [Test]
        public void ShouldIgnoreText() {
            const string text = "foo bar baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo(text));
        }

        [Test]
        public void ShouldReplaceUrl() {
            const string text = "foo [url]bar[/url] baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo("foo <a href=\"bar\">bar</a> baz"));
        }

        [Test]
        public void ShouldReplaceImg() {
            const string text = "foo [img]bar[/img] baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo("foo <img src=\"bar\" /> baz"));
        }

        [Test]
        public void ShouldReplaceUrlWithTitle() {
            const string text = "foo [url=alink]bar[/url] baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo("foo <a href=\"alink\">bar</a> baz"));
        }

        [Test]
        public void ShouldIgnoreMalformedUrl() {
            const string text = "foo [url]bar baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo("foo [url]bar baz"));
        }

        [Test]
        public void ShouldIgnoreMalformedUrlWithTitle() {
            const string text = "foo [url=alink]bar baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo("foo [url=alink]bar baz"));
        }

        [Test]
        public void ShouldIgnoreMalformedImg() {
            const string text = "foo [img]bar baz";
            var processed = _filter.ProcessContent(text, null);
            Assert.That(processed, Is.EqualTo("foo [img]bar baz"));
        }


    }
}