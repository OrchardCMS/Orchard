using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Orchard.Pages.Services.Templates;

namespace Orchard.Tests.Packages.Pages.Services.Templates {
    [TestFixture]
    public class TemplateMetadataParserTests {
        [Test]
        public void ParserShouldReturnEmptyListForEmptyMetadata() {
            var reader = new StringReader("  \r\n   ");
            var parser = new TemplateMetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParserShouldIgnoreEmptyTags() {
            var reader = new StringReader("  : test value \r\n   ");
            var parser = new TemplateMetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParserShouldReturnMetadata() {
            var reader = new StringReader("Description: test");
            var parser = new TemplateMetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Tag, Is.EqualTo("Description"));
            Assert.That(result[0].Value, Is.EqualTo("test"));
        }

        [Test]
        public void ParserShouldReturnMultiMetadata() {
            var reader = new StringReader("Description: test\r\nTag2: this is my test  ");
            var parser = new TemplateMetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Tag, Is.EqualTo("Description"));
            Assert.That(result[0].Value, Is.EqualTo("test"));
            Assert.That(result[1].Tag, Is.EqualTo("Tag2"));
            Assert.That(result[1].Value, Is.EqualTo("this is my test"));
        }

        [Test]
        public void ParserShouldSupportMultiLineValues() {
            var reader = new StringReader("Description: test    Tag2   this\r\n is my test\r\nName:\r\n FooBar");
            var parser = new TemplateMetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Tag, Is.EqualTo("Description"));
            Assert.That(result[0].Value, Is.EqualTo("test    Tag2   this is my test"));
            Assert.That(result[1].Tag, Is.EqualTo("Name"));
            Assert.That(result[1].Value, Is.EqualTo("FooBar"));
        }
    }
}