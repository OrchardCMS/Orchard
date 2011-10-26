using NUnit.Framework;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    public class StringExtensionsTests {
        [Test]
        public void CamelFriendly_CamelCasedStringMadeFriendly() {
            const string aCamel = "aCamel";
            Assert.That(aCamel.CamelFriendly(), Is.StringMatching("a Camel"));
        }
        [Test]
        public void CamelFriendly_PascalCasedStringMadeFriendly() {
            const string aCamel = "ACamel";
            Assert.That(aCamel.CamelFriendly(), Is.StringMatching("A Camel"));
        }
        [Test]
        public void CamelFriendly_LowerCasedStringMadeFriendly() {
            const string aCamel = "acamel";
            Assert.That(aCamel.CamelFriendly(), Is.StringMatching("acamel"));
        }
        [Test]
        public void CamelFriendly_EmptyStringReturnsEmptyString() {
            const string aCamel = "";
            Assert.That(aCamel.CamelFriendly(), Is.StringMatching(""));
        }
        [Test]
        public void CamelFriendly_NullValueReturnsEmptyString() {
            const string aCamel = null;
            Assert.That(aCamel.CamelFriendly(), Is.StringMatching(""));
        }

        [Test]
        public void Ellipsize_ShouldTuncateToTheExactNumber() {
            const string toEllipsize = "Lorem ipsum";
            Assert.That(toEllipsize.Ellipsize(2, ""), Is.StringMatching("Lo"));
            Assert.That(toEllipsize.Ellipsize(1, ""), Is.StringMatching("L"));
            Assert.That(toEllipsize.Ellipsize(0, ""), Is.StringMatching(""));
        }
        
        [Test]
        public void Ellipsize_TruncatedToWordBoundary() {
            const string toEllipsize = "Lorem ipsum";
            Assert.That(toEllipsize.Ellipsize(8, ""), Is.StringMatching("Lorem"));
            Assert.That(toEllipsize.Ellipsize(6, ""), Is.StringMatching("Lorem"));
            Assert.That(toEllipsize.Ellipsize(5, ""), Is.StringMatching("Lorem"));
            Assert.That(toEllipsize.Ellipsize(4, ""), Is.StringMatching(""));
        }

        [Test]
        public void Ellipsize_LongStringTruncatedToNearestWord() {
            const string toEllipsize = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam.";
            Assert.That(toEllipsize.Ellipsize(45), Is.StringMatching("Lorem ipsum dolor sit amet, consectetur&#160;&#8230;"));
        }

        [Test]
        public void Ellipsize_ShortStringReturnedAsSame() {
            const string toEllipsize = "Lorem ipsum";
            Assert.That(toEllipsize.Ellipsize(45), Is.StringMatching("Lorem ipsum"));
        }
        [Test]
        public void Ellipsize_EmptyStringReturnsEmptyString() {
            const string toEllipsize = "";
            Assert.That(toEllipsize.Ellipsize(45), Is.StringMatching(""));
        }
        [Test]
        public void Ellipsize_NullValueReturnsEmptyString() {
            const string toEllipsize = null;
            Assert.That(toEllipsize.Ellipsize(45), Is.StringMatching(""));
        }
        [Test]
        public void Ellipsize_CustomEllipsisStringIsUsed() {
            const string toEllipsize = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam.";
            Assert.That(toEllipsize.Ellipsize(45, "........"), Is.StringMatching("Lorem ipsum dolor sit amet, consectetur........"));
        }

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
        public void HtmlClassify_EmptyStringReturnsEmptyString() {
            const string toClassify = "";
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching(""));
        }
        [Test]
        public void HtmlClassify_NullValueReturnsEmptyString() {
            const string toClassify = null;
            Assert.That(toClassify.HtmlClassify(), Is.StringMatching(""));
        }

        [Test]
        public void OrDefault_ReturnsDefaultForNull() {
            const string s = null;
            var def = new LocalizedString("test");
            Assert.That(s.OrDefault(def).Text, Is.SameAs("test"));
        }
        [Test]
        public void OrDefault_ReturnsDefaultIfEmpty() {
            var def = new LocalizedString("test");
            Assert.That("".OrDefault(def).Text, Is.SameAs("test"));
        }
        [Test]
        public void OrDefault_ReturnsDefaultIfNull() {
            var def = new LocalizedString("test");
            Assert.That(((string)null).OrDefault(def).Text, Is.SameAs("test"));
        }
        [Test]
        public void OrDefault_ReturnsString() {
            var def = new LocalizedString("test");
            Assert.That("bar".OrDefault(def).Text, Is.SameAs("bar"));
        }

        [Test]
        public void RemoveTags_StringWithNoTagsReturnsSame() {
            const string fullOfTags = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam.";
            Assert.That(fullOfTags.RemoveTags(), Is.StringMatching("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void RemoveTags_SimpleWellFormedTagsAreRemoved() {
            const string fullOfTags = @"<p><em>Lorem ipsum</em> dolor sit amet, consectetur <a href=""#"">adipiscing</a> elit. Maecenas sed purus quis purus orci aliquam.</p>";
            Assert.That(fullOfTags.RemoveTags(), Is.StringMatching("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void RemoveTags_EmptyStringReturnsEmptyString() {
            const string fullOfTags = "";
            Assert.That(fullOfTags.RemoveTags(), Is.StringMatching(""));
        }
        [Test]
        public void RemoveTags_NullValueReturnsEmptyString() {
            const string fullOfTags = null;
            Assert.That(fullOfTags.RemoveTags(), Is.StringMatching(""));
        }

        [Test]
        public void ReplaceNewLinesWith_ReplaceCRLFWithHtmlBR() {
            const string lotsOfLineFeeds = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\r\nMaecenas sed purus quis purus orci aliquam.";
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith("<br />"), Is.StringMatching("Lorem ipsum dolor sit amet, consectetur adipiscing elit.<br />Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void ReplaceNewLinesWith_ReplaceCRLFWithHtmlPsAndCRLF() {
            const string lotsOfLineFeeds = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\r\nMaecenas sed purus quis purus orci aliquam.";
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith(@"</p>$1<p>"), Is.StringMatching("Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>\r\n<p>Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void ReplaceNewLinesWith_EmptyStringReturnsEmptyString() {
            const string lotsOfLineFeeds = "";
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith("<br />"), Is.StringMatching(""));
        }
        [Test]
        public void ReplaceNewLinesWith_NullValueReturnsEmptyString() {
            const string lotsOfLineFeeds = null;
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith("<br />"), Is.StringMatching(""));
        }
    }
}
