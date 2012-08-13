using NUnit.Framework;
using Orchard.Media.Helpers;

namespace Orchard.Tests.Modules.Media.Extensions {
    [TestFixture]
    public class MediaHelpersTests {
        [Test]
        public void PicturesArePictures() {
            Assert.That(MediaHelpers.IsPicture(null, "image.gif"), Is.True);
            Assert.That(MediaHelpers.IsPicture(null, "image.jpg"), Is.True);
            Assert.That(MediaHelpers.IsPicture(null, "image.jpeg"), Is.True);
            Assert.That(MediaHelpers.IsPicture(null, "image.png"), Is.True);
            Assert.That(MediaHelpers.IsPicture(null, "image.bmp"), Is.True);
            Assert.That(MediaHelpers.IsPicture(null, "image.ico"), Is.True);
        }

        [Test]
        public void PdfIsNotAPicture() {
            Assert.That(MediaHelpers.IsPicture(null, "notanimage.pdf"), Is.False);
        }
    }
}
