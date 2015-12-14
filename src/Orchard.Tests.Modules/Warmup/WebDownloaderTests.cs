using System.Net;
using NUnit.Framework;
using Orchard.Warmup.Services;

namespace Orchard.Tests.Modules.Warmup {
    public class WebDownloaderTests {
        private readonly IWebDownloader _webDownloader = new WebDownloader();

        [Test]
        public void ShouldReturnNullWhenUrlIsEmpty() {
            Assert.That(_webDownloader.Download(null), Is.Null);
            Assert.That(_webDownloader.Download(""), Is.Null);
            Assert.That(_webDownloader.Download(" "), Is.Null);
        }

        [Test]
        public void ShouldReturnNullWhenUrlIsInvalid() {
            Assert.That(_webDownloader.Download("froutfrout|yepyep"), Is.Null);
        }

        [Test]
        public void StatusCodeShouldBe404ForUnexistingResources() {
            var download = _webDownloader.Download("http://orchardproject.net/yepyep");
            Assert.That(download, Is.Not.Null);
            Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(download.Content, Is.Null);
        }

        [Test]
        public void StatusCodeShouldBe200ForValidRequests() {
            var download = _webDownloader.Download("http://orchardproject.net/");
            Assert.That(download, Is.Not.Null);
            Assert.That(download.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(download.Content, Is.Not.Empty);
        }
    }
}
