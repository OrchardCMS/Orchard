using NUnit.Framework;
using Orchard.CmsPages.Services.Templates;

namespace Orchard.Tests.Packages.Pages.Services.Templates {
    [TestFixture]
    public class TemplateProviderTests {
        private StubTemplateEntryProvider _entryProvider;
        private ITemplateProvider _provider;

        [SetUp]
        public void Init() {
            _entryProvider = new StubTemplateEntryProvider();
            _provider = new TemplateProvider(_entryProvider, new TemplateMetadataParser());
            _entryProvider.AddTemplate("test1", @"
<%@Page %>
<%--
name: Two column layout
description: This has a main content area and a sidebar on the right.
zones: Content, Right sidebar
author: Jon
OtherTag1: OtherValue1
OtherTag2: OtherValue2
--%>
");
        }

        [Test]
        public void ProviderShouldReturnTemplates() {
            var templateDescriptors = _provider.List();
            Assert.That(templateDescriptors.Count, Is.EqualTo(1));
            Assert.That(templateDescriptors[0].Name, Is.EqualTo("test1"));
            Assert.That(templateDescriptors[0].DisplayName, Is.EqualTo("Two column layout"));
            Assert.That(templateDescriptors[0].Description, Is.EqualTo("This has a main content area and a sidebar on the right."));
            Assert.That(templateDescriptors[0].Zones.Count, Is.EqualTo(2));
            Assert.That(templateDescriptors[0].Zones[0], Is.EqualTo("Content"));
            Assert.That(templateDescriptors[0].Zones[1], Is.EqualTo("Right sidebar"));
            Assert.That(templateDescriptors[0].Author, Is.EqualTo("Jon"));
            Assert.That(templateDescriptors[0].Others.Count, Is.EqualTo(2));
            Assert.That(templateDescriptors[0].Others[0].Tag, Is.EqualTo("OtherTag1"));
            Assert.That(templateDescriptors[0].Others[0].Value, Is.EqualTo("OtherValue1"));
            Assert.That(templateDescriptors[0].Others[1].Tag, Is.EqualTo("OtherTag2"));
            Assert.That(templateDescriptors[0].Others[1].Value, Is.EqualTo("OtherValue2"));
        }

        [Test]
        public void GetShouldLocateTemplateDescriptorByNameOrReturnNull() {
            var test1= _provider.Get("test1");
            var test2 = _provider.Get("test2");

            Assert.That(test1, Is.Not.Null);
            Assert.That(test2, Is.Null);
        }
    }
}