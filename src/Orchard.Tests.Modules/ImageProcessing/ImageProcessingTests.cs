using System.Reflection;
using Moq;
using NUnit.Framework;
using Orchard.MediaProcessing.Services;

namespace Orchard.Tests.Modules.ImageProcessing {
    [TestFixture]
    public class ImageProcessingTests {

        private readonly MethodInfo _createDefaultFilename = typeof(ImageProfileManager).GetMethod("CreateDefaultFileName", BindingFlags.NonPublic | BindingFlags.Static);

        private string CreateDefaultFileName(string path) {
            return (string)_createDefaultFilename.Invoke(null, new object[] {path});
        }

        [Test]
        public void CreateDefaultFilenameRemovesInvalidChars() {
            Assert.That(CreateDefaultFileName("abcdef"), Is.EqualTo("abcdef"));
            Assert.That(CreateDefaultFileName("abc_def"), Is.EqualTo("abc_def"));
        }

        [Test]
        public void CreateDefaultFilenamePeservesInternationalLetters() {
            Assert.That(CreateDefaultFileName("aéçâê"), Is.EqualTo("aecae"));
            Assert.That(CreateDefaultFileName("джинсы_клеш"), Is.EqualTo("джинсы_клеш"));
        }
    }
}
