using System.Collections.Specialized;
using System.Web;
using NUnit.Framework;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    public class HttpRequestExtensionsTests {
    
        [Test]
        public void IsLocalUrlShouldReturnFalseWhenUrlIsNullOrEmpty() {
            var request = new StubHttpRequest();

            Assert.That(request.IsLocalUrl(null), Is.False);
            Assert.That(request.IsLocalUrl("   "), Is.False);
            Assert.That(request.IsLocalUrl(""), Is.False);
        }

        [Test]
        public void IsLocalUrlShouldReturnFalseWhenUrlStartsWithDoubleSlash() {
            var request = new StubHttpRequest();

            Assert.That(request.IsLocalUrl("//"), Is.False);
        }

        [Test]
        public void IsLocalUrlShouldReturnFalseWhenUrlStartsWithForwardBackwardSlash() {
            var request = new StubHttpRequest();

            Assert.That(request.IsLocalUrl("/\\"), Is.False);
        }

        [Test]
        public void IsLocalUrlShouldReturnTrueWhenUrlStartsWithSlashAndAnythingElse() {
            var request = new StubHttpRequest();

            Assert.That(request.IsLocalUrl("/"), Is.True);
            Assert.That(request.IsLocalUrl("/контакты"), Is.True);
            Assert.That(request.IsLocalUrl("/  "), Is.True);
            Assert.That(request.IsLocalUrl("/abc-def"), Is.True);
        }

        [Test]
        public void IsLocalUrlShouldReturnTrueWhenAuthoritiesMatch() {
            var request = new StubHttpRequest();
            request.Headers.Add("Host", "localhost");

            Assert.That(request.IsLocalUrl("http://localhost"), Is.True);
        }

        [Test]
        public void IsLocalUrlShouldReturnFalseWhenAuthoritiesDiffer() {
            var request = new StubHttpRequest();
            request.Headers.Add("Host", "localhost");

            Assert.That(request.IsLocalUrl("http://somedomain"), Is.False);
            Assert.That(request.IsLocalUrl("http://localhost:8080"), Is.False);
        }

        [Test]
        public void IsLocalUrlShouldReturnFalseForEverythingElse() {
            var request = new StubHttpRequest();
            request.Headers.Add("Host", "localhost");

            Assert.That(request.IsLocalUrl("abc"), Is.False);
        }
    }

    class StubHttpRequest : HttpRequestBase {
        private readonly NameValueCollection _headers = new NameValueCollection();

        public override NameValueCollection Headers {
            get {
                return _headers;
            }
        }
    }
}
