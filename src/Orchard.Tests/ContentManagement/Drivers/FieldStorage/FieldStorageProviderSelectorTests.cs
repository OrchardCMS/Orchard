using System;
using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.Tests.ContentManagement.Drivers.FieldStorage {
    [TestFixture]
    public class FieldStorageProviderSelectorTests {
        private IContainer _container;
        private IFieldStorageProviderSelector _selector;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<FieldStorageProviderSelector>().As<IFieldStorageProviderSelector>();
            builder.RegisterType<InfosetStorageProvider>().As<IFieldStorageProvider>();
            builder.RegisterType<TestProvider>().As<IFieldStorageProvider>();

            _container = builder.Build();
            _selector = _container.Resolve<IFieldStorageProviderSelector>();
        }

        class TestProvider : IFieldStorageProvider {
            public string ProviderName {
                get { return "TestProvName"; }
            }

            public IFieldStorage BindStorage(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition) {
                throw new NotImplementedException();
            }

        }

        [Test]
        public void ShouldReturnProviderByName() {
            var part1Definition = new ContentPartDefinitionBuilder()
                .WithField("Hello", fb => fb.OfType("Text").WithSetting("Storage", "TestProvName"))
                .Build();
            var part2Definition = new ContentPartDefinitionBuilder()
                .WithField("World", fb => fb.OfType("Text").WithSetting("Storage", "Infoset"))
                .Build();

            var provider1 = _selector.GetProvider(part1Definition.Fields.Single());
            var provider2 = _selector.GetProvider(part2Definition.Fields.Single());

            Assert.That(provider1.ProviderName, Is.EqualTo("TestProvName"));
            Assert.That(provider2.ProviderName, Is.EqualTo("Infoset"));
        }
        
        [Test]
        public void ShouldReturnInfosetWhenNullEmptyMissingOrInvalid() {
            var part1Definition = new ContentPartDefinitionBuilder()
                .WithField("Hello", fb => fb.OfType("Text").WithSetting("Storage", null))
                .Build();
            var part2Definition = new ContentPartDefinitionBuilder()
                .WithField("World", fb => fb.OfType("Text").WithSetting("Storage", ""))
                .Build();
            var part3Definition = new ContentPartDefinitionBuilder()
                .WithField("World", fb => fb.OfType("Text"))
                .Build();
            var part4Definition = new ContentPartDefinitionBuilder()
                .WithField("World", fb => fb.OfType("Text").WithSetting("Storage", "NoSuchName"))
                .Build();

            var provider1 = _selector.GetProvider(part1Definition.Fields.Single());
            var provider2 = _selector.GetProvider(part2Definition.Fields.Single());
            var provider3 = _selector.GetProvider(part3Definition.Fields.Single());
            var provider4 = _selector.GetProvider(part4Definition.Fields.Single());

            Assert.That(provider1.ProviderName, Is.EqualTo("Infoset"));
            Assert.That(provider2.ProviderName, Is.EqualTo("Infoset"));
            Assert.That(provider3.ProviderName, Is.EqualTo("Infoset"));
            Assert.That(provider4.ProviderName, Is.EqualTo("Infoset"));
        }
    }
}
