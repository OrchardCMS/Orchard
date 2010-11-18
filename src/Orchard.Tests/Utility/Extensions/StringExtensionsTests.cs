using NUnit.Framework;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    public class StringExtensionsTests {
        [Test]
        public void HtmlClassify_ValidReallySimpleClassNameReturnsSame() {
            const string toClassify = "someclass";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching(toClassify));
        }
        [Test]
        public void HtmlClassify_ValidSimpleClassNameReturnsSame() {
            const string toClassify = "some-class";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching(toClassify));
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
        public void HtmlClassify_LowerCamelCasedStringReturnsLowerHyphenatedClassName() {
            const string toClassify = "camelCased";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching("camel-cased"));
        }
        [Test]
        public void HtmlClassify_PascalCasedStringReturnsLowerHyphenatedClassName() {
            const string toClassify = "PascalCased";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching("pascal-cased"));
        }
        [Test]
        public void OrDefault_ReturnsDefaultForNull() {
            const string s = null;
            var def = new LocalizedString("test");
            Assert.That(s.OrDefault(def).Text, Is.SameAs("test"));
        }
        [Test]
        public void OrDefault_ReturnsDefault() {
            var def = new LocalizedString("test");
            Assert.That("".OrDefault(def).Text, Is.SameAs("test"));
        }

        [Test]
        public void OrDefault_ReturnsString() {
            var def = new LocalizedString("test");
            Assert.That("bar".OrDefault(def).Text, Is.SameAs("bar"));
        }
    }
}
