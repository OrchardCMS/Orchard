using Moq;
using NUnit.Framework;
using Orchard.FileSystems.VirtualPath;

namespace Orchard.Tests.FileSystems.VirtualPath {
    [TestFixture]
    public class DefaultVirtualPathProviderTests {
        [Test]
        public void TryFileExistsTest() {
            StubDefaultVirtualPathProvider defaultVirtualPathProvider = new StubDefaultVirtualPathProvider();

            Assert.That(defaultVirtualPathProvider.TryFileExists("~/a.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.TryFileExists("~/../a.txt"), Is.False);
            Assert.That(defaultVirtualPathProvider.TryFileExists("~/a/../a.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.TryFileExists("~/a/b/../a.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.TryFileExists("~/a/b/../../a.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.TryFileExists("~/a/b/../../../a.txt"), Is.False);
            Assert.That(defaultVirtualPathProvider.TryFileExists("~/a/../../b/c.txt"), Is.False);
        }

        [Test]
        public void RejectMalformedVirtualPathTests() {
            StubDefaultVirtualPathProvider defaultVirtualPathProvider = new StubDefaultVirtualPathProvider();

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/a.txt"), Is.False);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/a.txt"), Is.False);

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/../a.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/../a.txt"), Is.True);

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/a/../a.txt"), Is.False);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/a/../a.txt"), Is.False);

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/a/b/../a.txt"), Is.False);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/a/b/../a.txt"), Is.False);

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/a/b/../../a.txt"), Is.False);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/a/b/../../a.txt"), Is.False);

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/a/b/../../../a.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/a/b/../../../a.txt"), Is.True);

            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("~/a/../../b//.txt"), Is.True);
            Assert.That(defaultVirtualPathProvider.RejectMalformedVirtualPath("/a/../../b//.txt"), Is.True);
        }
    }

    internal class StubDefaultVirtualPathProvider : DefaultVirtualPathProvider {
        public override bool FileExists(string path) {
            return true;
        }
    }
}
