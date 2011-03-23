using System;
using NUnit.Framework;
using Orchard.Environment.Warmup;

namespace Orchard.Tests.Environment.Warmup {
    [TestFixture]
    public class WarmupUtilityTests {

        [Test]
        public void EmptyStringsAreNotAllowed() {
            Assert.Throws<ArgumentException>(() => WarmupUtility.EncodeUrl(""));
            Assert.Throws<ArgumentException>(() => WarmupUtility.EncodeUrl(null));
        }

        [Test]
        public void EncodedUrlsShouldBeValidFilenames() {
            Assert.That(WarmupUtility.EncodeUrl("http://www.microsoft.com"), Is.EqualTo("http_3A_2F_2Fwww_2Emicrosoft_2Ecom"));
            Assert.That(WarmupUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz"), Is.EqualTo("http_3A_2F_2Fwww_2Emicrosoft_2Ecom_2Ffoo_3Fbar_3Dbaz"));
        }

        [Test]
        public void EncodedUrlsShouldPreserveQueryStrings() {
            Assert.That(WarmupUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz"), Is.StringContaining("bar"));
            Assert.That(WarmupUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz"), Is.StringContaining("baz"));
            Assert.That(WarmupUtility.EncodeUrl("http://www.microsoft.com/foo?bar=baz"), Is.StringContaining("foo"));
        }
    }
}
