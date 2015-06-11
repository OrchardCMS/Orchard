using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Services;

namespace Orchard.Tests.ContentManagement.MetaData.Services {
    [TestFixture]
    public class ContentDefinitionWriterTests {
        private ContentDefinitionWriter _writer;

        [SetUp]
        public void Init() {
            _writer = new ContentDefinitionWriter(new SettingsFormatter());
        }

        [Test]
        public void CreatesElementWithEncodedContentTypeName() {
            var alphaInfoset = _writer.Export(new ContentTypeDefinitionBuilder().Named("alpha").Build());
            var betaInfoset = _writer.Export(new ContentTypeDefinitionBuilder().Named(":beta").Build());
            var gammaInfoset = _writer.Export(new ContentTypeDefinitionBuilder().Named(" g a m m a ").Build());
            var deltaInfoset = _writer.Export(new ContentTypeDefinitionBuilder().Named("del\r\nta").Build());

            Assert.That(XmlConvert.DecodeName(alphaInfoset.Name.LocalName), Is.EqualTo("alpha"));
            Assert.That(XmlConvert.DecodeName(betaInfoset.Name.LocalName), Is.EqualTo(":beta"));
            Assert.That(XmlConvert.DecodeName(gammaInfoset.Name.LocalName), Is.EqualTo(" g a m m a "));
            Assert.That(XmlConvert.DecodeName(deltaInfoset.Name.LocalName), Is.EqualTo("del\r\nta"));
        }

        [Test]
        public void ChildElementsArePartNames() {
            var alphaInfoset = _writer.Export(new ContentTypeDefinitionBuilder().Named("alpha").WithPart(":beta").WithPart("del\r\nta").Build());

            Assert.That(XmlConvert.DecodeName(alphaInfoset.Name.LocalName), Is.EqualTo("alpha"));
            Assert.That(alphaInfoset.Elements().Count(), Is.EqualTo(2));
            Assert.That(alphaInfoset.Elements().Select(elt => elt.Name.LocalName), Has.Some.EqualTo(XmlConvert.EncodeLocalName(":beta")));
            Assert.That(alphaInfoset.Elements().Select(elt => elt.Name.LocalName), Has.Some.EqualTo(XmlConvert.EncodeLocalName("del\r\nta")));
        }

        [Test]
        public void TypeAndTypePartSettingsAreAttributes() {

            var alpha = new ContentTypeDefinitionBuilder()
                .Named("alpha")
                .WithSetting("x", "1")
                .WithPart("beta", part => part.WithSetting(" y ", "2"))
                .Build();

            var alphaInfoset = _writer.Export(alpha);
            Assert.That(alphaInfoset.Attributes("x").Single().Value, Is.EqualTo("1"));
            Assert.That(alphaInfoset.Elements("beta").Attributes(XmlConvert.EncodeLocalName(" y ")).Single().Value, Is.EqualTo("2"));
        }
    }
}
