using NUnit.Framework;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    public class StringExtensionsTests {
        [Test]
        public void OrDefault_ReturnsDefaultForNull() {
            const string s = null;
            Assert.That(s.OrDefault("test"), Is.SameAs("test"));
        }
        [Test]
        public void OrDefault_ReturnsDefault() {
            Assert.That("".OrDefault("test"), Is.SameAs("test"));
        }

        [Test]
        public void OrDefault_ReturnsString() {
            Assert.That("bar".OrDefault("test"), Is.SameAs("bar"));
        }
        [Test]
        public void IsNullOrEmptyTrimmed_EmptyStringReturnsTrue() {
            const string testString = "";
            Assert.AreEqual(true, testString.IsNullOrEmptyTrimmed());
        }

        [Test]
        public void IsNullOrEmptyTrimmed_NullStringReturnsTrue() {
            const string testString = null;
            Assert.AreEqual(true, testString.IsNullOrEmptyTrimmed());
        }

        [Test]
        public void IsNullOrEmptyTrimmed_SpacedStringReturnsTrue() {
            const string testString = "    ";
            Assert.AreEqual(true, testString.IsNullOrEmptyTrimmed());
        }

        [Test]
        public void IsNullOrEmptyTrimmed_ActualStringReturnsFalse() {
            const string testString = "testString";
            Assert.AreEqual(false, testString.IsNullOrEmptyTrimmed());
        }
    }
}
