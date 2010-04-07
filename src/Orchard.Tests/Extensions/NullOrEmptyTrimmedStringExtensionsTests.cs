using NUnit.Framework;
using Orchard.Extensions;

namespace Orchard.Tests.Extensions
{
    [TestFixture]
    public class NullOrEmptyTrimmedStringExtensionsTests
    {
        [Test]
        public void Trimmed_EmptyStringReturnsTrue() {
            const string testString = "";
            Assert.AreEqual(true, testString.IsNullOrEmptyTrimmed());
        }

        [Test]
        public void NullStringReturnsTrue() {
            const string testString = null;
            Assert.AreEqual(true, testString.IsNullOrEmptyTrimmed());
        }

        [Test]
        public void SpacedStringReturnsTrue() {
            const string testString = "    ";
            Assert.AreEqual(true, testString.IsNullOrEmptyTrimmed());
        }

        [Test]
        public void ActualStringReturnsFalse() {
            const string testString = "testString";
            Assert.AreEqual(false, testString.IsNullOrEmptyTrimmed());
        }
    }
}
