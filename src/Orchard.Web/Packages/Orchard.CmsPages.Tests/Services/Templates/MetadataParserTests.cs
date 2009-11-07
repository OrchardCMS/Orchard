using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Orchard.CmsPages.Services.Templates;

namespace Orchard.CmsPages.Tests.Services.Templates {
    [TestFixture]
    public class MetadataParserTests {
        [Test]
        public void ParserShouldReturnEmptyListForEmptyMetadata() {
            var reader = new StringReader("  \r\n   ");
            var parser = new MetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParserShouldIgnoreEmptyTags() {
            var reader = new StringReader("  : test value \r\n   ");
            var parser = new MetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ParserShouldReturnMetadata() {
            var reader = new StringReader("Description: test");
            var parser = new MetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Tag, Is.EqualTo("Description"));
            Assert.That(result[0].Value, Is.EqualTo("test"));
        }

        [Test]
        public void ParserShouldReturnMultiMetadata() {
            var reader = new StringReader("Description: test\r\n     Tag2  : this is my test");
            var parser = new MetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Tag, Is.EqualTo("Description"));
            Assert.That(result[0].Value, Is.EqualTo("test"));
            Assert.That(result[1].Tag, Is.EqualTo("Tag2"));
            Assert.That(result[1].Value, Is.EqualTo("this is my test"));
        }

        [Test]
        public void ParserShouldIgnoreTagsNotPreceededByNewLine() {
            var reader = new StringReader("Description: test    Tag2  : this is \r\n my test  \r\n  ");
            var parser = new MetadataParser();
            IList<MetadataEntry> result = parser.Parse(reader);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Tag, Is.EqualTo("Description"));
            Assert.That(result[0].Value, Is.EqualTo("test    Tag2  : this is \r\n my test"));
        }
    }
}