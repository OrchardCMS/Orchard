using NUnit.Framework;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    public class StringExtensionsTests {
        [Test]
        public void HtmlClassify_ValidSimpleClassNameReturnsSame() {
            const string toClassify = "some-class";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching("some-class"));
        }
        [Test]
        public void HtmlClassify_SimpleStringReturnsSimpleClassName() {
            const string toClassify = "this is something";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching("this-is-something"));
        }
        [Test]
        public void HtmlClassify_ValidComplexClassNameReturnsSimpleClassName() {
            const string toClassify = @"some-class\&some.other.class";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching("some-class-some-other-class"));
        }
        [Test]
        public void HtmlClassify_CompletelyInvalidClassNameReturnsEmptyString() {
            const string toClassify = @"0_1234_12";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching(""));
        }
        [Test]
        public void OrDefault_ReturnsDefaultForNull() {
            const string s = null;
            Assert.That(s.OrDefault(new LocalizedString("test")), Is.SameAs("test"));
        }
        [Test]
        public void OrDefault_ReturnsDefault() {
            Assert.That("".OrDefault(new LocalizedString("test")), Is.SameAs("test"));
        }

        [Test]
        public void OrDefault_ReturnsString() {
            Assert.That("bar".OrDefault(new LocalizedString("test")), Is.SameAs("bar"));
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
