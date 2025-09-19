using System;
using NUnit.Framework;
using Orchard.Localization;
using Orchard.Utility.Extensions;

namespace Orchard.Tests.Utility.Extensions {
    [TestFixture]
    public class StringExtensionsTests {
        [Test]
        public void CamelFriendly_CamelCasedStringMadeFriendly() {
            const string aCamel = "aCamel";
            Assert.That(aCamel.CamelFriendly(), Does.Match("a Camel"));
        }
        [Test]
        public void CamelFriendly_PascalCasedStringMadeFriendly() {
            const string aCamel = "ACamel";
            Assert.That(aCamel.CamelFriendly(), Does.Match("A Camel"));
        }
        [Test]
        public void CamelFriendly_LowerCasedStringMadeFriendly() {
            const string aCamel = "acamel";
            Assert.That(aCamel.CamelFriendly(), Does.Match("acamel"));
        }
        [Test]
        public void CamelFriendly_EmptyStringReturnsEmptyString() {
            const string aCamel = "";
            Assert.That(aCamel.CamelFriendly(), Does.Match(""));
        }
        [Test]
        public void CamelFriendly_NullValueReturnsEmptyString() {
            const string aCamel = null;
            Assert.That(aCamel.CamelFriendly(), Does.Match(""));
        }

        [Test]
        public void Ellipsize_ShouldTuncateToTheExactNumber() {
            const string toEllipsize = "Lorem ipsum";
            Assert.That(toEllipsize.Ellipsize(2, ""), Is.EqualTo("Lo"));
            Assert.That(toEllipsize.Ellipsize(1, ""), Is.EqualTo("L"));
            Assert.That(toEllipsize.Ellipsize(0, ""), Is.EqualTo(""));
        }
        
        [Test]
        public void Ellipsize_TruncatedToWordBoundary() {
            const string toEllipsize = "Lorem ipsum";
            Assert.That(toEllipsize.Ellipsize(8, ""), Is.EqualTo("Lorem"));
            Assert.That(toEllipsize.Ellipsize(6, ""), Is.EqualTo("Lorem"));
            Assert.That(toEllipsize.Ellipsize(5, ""), Is.EqualTo("Lorem"));
            Assert.That(toEllipsize.Ellipsize(4, ""), Is.EqualTo("Lore"));
        }

        [Test]
        public void Ellipsize_LongStringTruncatedToNearestWord() {
            const string toEllipsize = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam.";
            Assert.That(toEllipsize.Ellipsize(46), Does.Match("Lorem ipsum dolor sit amet, consectetur\u00A0\u2026"));
        }

        [Test]
        public void Ellipsize_ShortStringReturnedAsSame() {
            const string toEllipsize = "Lorem ipsum";
            Assert.That(toEllipsize.Ellipsize(45), Does.Match("Lorem ipsum"));
        }
        [Test]
        public void Ellipsize_EmptyStringReturnsEmptyString() {
            const string toEllipsize = "";
            Assert.That(toEllipsize.Ellipsize(45), Does.Match(""));
        }
        [Test]
        public void Ellipsize_NullValueReturnsEmptyString() {
            const string toEllipsize = null;
            Assert.That(toEllipsize.Ellipsize(45), Does.Match(""));
        }
        [Test]
        public void Ellipsize_CustomEllipsisStringIsUsed() {
            const string toEllipsize = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam.";
            Assert.That(toEllipsize.Ellipsize(45, "........"), Does.Match("Lorem ipsum dolor sit amet, consectetur........"));
        }
        [Test]
        public void Ellipsize_WordBoundary() {
            const string toEllipsize = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam.";
            Assert.That(toEllipsize.Ellipsize(43, "..."), Does.Match("Lorem ipsum dolor sit amet, consectet..."));
            Assert.That(toEllipsize.Ellipsize(43, "...", true), Does.Match("Lorem ipsum dolor sit amet, ..."));
        }

        [Test]
        public void HtmlClassify_ValidReallySimpleClassNameReturnsSame() {
            const string toClassify = "someclass";
            Assert.That(toClassify.HtmlClassify(), Does.Match(toClassify));
        }
        [Test]
        public void HtmlClassify_NumbersAreMaintainedIfNotAtStart() {
            const string toClassify = "some4class5";
            Assert.That(toClassify.HtmlClassify(), Does.Match(toClassify));
        }
        [Test]
        public void HtmlClassify_NumbersAreStrippedAtStart() {
            const string toClassify = "5someClass";
            Assert.That(toClassify.HtmlClassify(), Does.Match("some-class"));
        }
        [Test]
        public void HtmlClassify_ValidSimpleClassNameReturnsSame() {
            const string toClassify = "some-class";
            Assert.That(toClassify.HtmlClassify(), Does.Match(toClassify));
        }
        [Test]
        public void HtmlClassify_SimpleStringReturnsSimpleClassName() {
            const string toClassify = "this is something";
            Assert.That(toClassify.HtmlClassify(), Does.Match("this-is-something"));
        }
        [Test]
        public void HtmlClassify_ValidComplexClassNameReturnsSimpleClassName() {
            const string toClassify = @"some-class\&some.other.class";
            Assert.That(toClassify.HtmlClassify(), Does.Match("some-class-some-other-class"));
        }
        [Test]
        public void HtmlClassify_CompletelyInvalidClassNameReturnsEmptyString() {
            const string toClassify = @"0_1234_12";
            Assert.That(toClassify.HtmlClassify(), Does.Match(""));
        }
        [Test]
        public void HtmlClassify_LowerCamelCasedStringReturnsLowerHyphenatedClassName() {
            const string toClassify = "camelCased";
            Assert.That(toClassify.HtmlClassify(), Does.Match("camel-cased"));
        }
        [Test]
        public void HtmlClassify_PascalCasedStringReturnsLowerHyphenatedClassName() {
            const string toClassify = "PascalCased";
            Assert.That(toClassify.HtmlClassify(), Does.Match("pascal-cased"));
        }
        [Test]
        public void HtmlClassify_EmptyStringReturnsEmptyString() {
            const string toClassify = "";
            Assert.That(toClassify.HtmlClassify(), Does.Match(""));
        }
        [Test]
        public void HtmlClassify_NullValueReturnsEmptyString() {
            const string toClassify = null;
            Assert.That(toClassify.HtmlClassify(), Does.Match(""));
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
            Assert.That(fullOfTags.RemoveTags(), Does.Match("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void RemoveTags_SimpleWellFormedTagsAreRemoved() {
            const string fullOfTags = @"<p><em>Lorem ipsum</em> dolor sit amet, consectetur <a href=""#"">adipiscing</a> elit. Maecenas sed purus quis purus orci aliquam.</p>";
            Assert.That(fullOfTags.RemoveTags(), Does.Match("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void RemoveTags_EmptyStringReturnsEmptyString() {
            const string fullOfTags = "";
            Assert.That(fullOfTags.RemoveTags(), Does.Match(""));
        }
        [Test]
        public void RemoveTags_NullValueReturnsEmptyString() {
            const string fullOfTags = null;
            Assert.That(fullOfTags.RemoveTags(), Does.Match(""));
        }

        [Test]
        public void ReplaceNewLinesWith_ReplaceCRLFWithHtmlBR() {
            const string lotsOfLineFeeds = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\r\nMaecenas sed purus quis purus orci aliquam.";
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith("<br />"), Does.Match("Lorem ipsum dolor sit amet, consectetur adipiscing elit.<br />Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void ReplaceNewLinesWith_ReplaceCRLFWithHtmlPsAndCRLF() {
            const string lotsOfLineFeeds = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.\r\nMaecenas sed purus quis purus orci aliquam.";
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith(@"</p>{0}<p>"), Does.Match("Lorem ipsum dolor sit amet, consectetur adipiscing elit.</p>\r\n<p>Maecenas sed purus quis purus orci aliquam."));
        }
        [Test]
        public void ReplaceNewLinesWith_EmptyStringReturnsEmptyString() {
            const string lotsOfLineFeeds = "";
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith("<br />"), Does.Match(""));
        }
        [Test]
        public void ReplaceNewLinesWith_NullValueReturnsEmptyString() {
            const string lotsOfLineFeeds = null;
            Assert.That(lotsOfLineFeeds.ReplaceNewLinesWith("<br />"), Does.Match(""));
        }

        [Test]
        public void StripShouldRemoveStart() {
            Assert.That("abc".Strip('a'), Does.Match("bc"));
            Assert.That("abc".Strip("ab".ToCharArray()), Does.Match("c"));
        }

        [Test]
        public void StripShouldRemoveInside() {
            Assert.That("abc".Strip('b'), Does.Match("ac"));
            Assert.That("abc".Strip("abc".ToCharArray()), Does.Match(""));
        }

        [Test]
        public void StripShouldRemoveEnd() {
            Assert.That("abc".Strip('c'), Does.Match("ab"));
            Assert.That("abc".Strip("bc".ToCharArray()), Does.Match("a"));
        }

        [Test]
        public void StripShouldReturnIfEmpty() {
            Assert.That("".Strip('a'), Does.Match(""));
            Assert.That("a".Strip("".ToCharArray()), Does.Match("a"));
        }

        [Test]
        public void AnyShouldReturnTrueAtStart() {
            Assert.That("abc".Any('a'), Is.True);
            Assert.That("abc".Any("ab".ToCharArray()), Is.True);
        }

        [Test]
        public void AnyShouldReturnTrueAtEnd() {
            Assert.That("abc".Any('c'), Is.True);
            Assert.That("abc".Any("bc".ToCharArray()), Is.True);
        }

        [Test]
        public void AnyShouldReturnTrueAtMiddle() {
            Assert.That("abc".Any('b'), Is.True);
            Assert.That("abc".Any("abc".ToCharArray()), Is.True);
        }

        [Test]
        public void AnyShouldReturnFalseIfNotPresent() {
            Assert.That("abc".Any("".ToCharArray()), Is.False);
            Assert.That("abc".Any("d".ToCharArray()), Is.False);
        }

        [Test]
        public void AllShouldReturnTrueIfAllArePresent() {
            Assert.That("abc".All("abc".ToCharArray()), Is.True);
            Assert.That("abc".All("abcd".ToCharArray()), Is.True);
            Assert.That("".All("a".ToCharArray()), Is.True);
            Assert.That("abc".All("abcd".ToCharArray()), Is.True);
        }

        [Test]
        public void AllShouldReturnFalseIfAnyIsNotPresent() {
            Assert.That("abc".All("".ToCharArray()), Is.False);
            Assert.That("abc".All("a".ToCharArray()), Is.False);
        }

        [Test]
        public void TranslateShouldThrowException() {
            Assert.Throws<ArgumentNullException>(() => "a".Translate("".ToCharArray(), "a".ToCharArray()));
            Assert.Throws<ArgumentNullException>(() => "a".Translate("a".ToCharArray(), "".ToCharArray()));
        }

        [Test]
        public void TranslateShouldReturnSource() {
            Assert.That("a".Translate("".ToCharArray(), "".ToCharArray()), Does.Match(""));
            Assert.That("".Translate("abc".ToCharArray(), "abc".ToCharArray()), Does.Match(""));
        }

        [Test]
        public void TranslateShouldReplaceChars() {
            Assert.That("abc".Translate("a".ToCharArray(), "d".ToCharArray()), Does.Match("dbc"));
            Assert.That("abc".Translate("d".ToCharArray(), "d".ToCharArray()), Does.Match("abc"));
            Assert.That("abc".Translate("abc".ToCharArray(), "def".ToCharArray()), Does.Match("def"));
        }

        [Test]
        public void ShouldEncodeToBase64() {
            Assert.That("abc".ToBase64(), Is.EqualTo("YWJj"));
        }

        [Test]
        public void ShouldDecodeFromBase64() {
            Assert.That("YWJj".FromBase64(), Is.EqualTo("abc"));
        }

        [Test]
        public void ShouldRoundtripBase64() {
            Assert.That("abc".ToBase64().FromBase64(), Is.EqualTo("abc"));
            Assert.That("YWJj".FromBase64().ToBase64(), Is.EqualTo("YWJj"));
        }
    }
}
