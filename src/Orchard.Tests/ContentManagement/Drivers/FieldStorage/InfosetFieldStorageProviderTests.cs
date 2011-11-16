using System.Linq;
using Autofac;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;

namespace Orchard.Tests.ContentManagement.Drivers.FieldStorage {
    public class InfosetFieldStorageProviderTests {
        private IContainer _container;
        private IFieldStorageProvider _provider;

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterType<InfosetStorageProvider>().As<IFieldStorageProvider>();

            _container = builder.Build();
            _provider = _container.Resolve<IFieldStorageProvider>();
        }

        private ContentPartDefinition FooPartDefinition() {
            return new ContentPartDefinitionBuilder()
                .Named("Foo")
                .WithField("Bar")
                .Build();
        }

        private ContentPart CreateContentItemPart() {
            var partDefinition = FooPartDefinition();
            var typeDefinition = new ContentTypeDefinitionBuilder()
                .WithPart(partDefinition, part => { })
                .Build();
            var contentItem = new ContentItem {
                VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord()
                }
            };
            var contentPart = new ContentPart {
                TypePartDefinition = typeDefinition.Parts.Single()
            };
            contentItem.Weld(contentPart);
            contentItem.Weld(new InfosetPart {
                Infoset = contentItem.Record.Infoset,
                VersionInfoset = contentItem.VersionRecord.Infoset
            });
            return contentPart;
        }

        [Test]
        public void BoundStorageIsNotNull() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());
            Assert.That(storage, Is.Not.Null);
        }

        [Test]
        public void GettingUnsetNamedAndUnnamedValueIsSafeAndNull() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());
            Assert.That(storage.Get<string>(null), Is.Null);
            Assert.That(storage.Get<string>("value"), Is.Null);
            Assert.That(storage.Get<string>("This is a test"), Is.Null);
        }

        [Test]
        public void ValueThatIsSetIsAlsoReturned() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            Assert.That(storage.Get<string>("alpha"), Is.Null);
            storage.Set("alpha", "one");
            Assert.That(storage.Get<string>("alpha"), Is.Not.Null);
            Assert.That(storage.Get<string>("alpha"), Is.EqualTo("one"));
        }

        [Test]
        public void NullAndEmptyValueNamesAreTreatedTheSame() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            Assert.That(storage.Get<string>(null), Is.Null);
            Assert.That(storage.Get<string>(""), Is.Null);
            storage.Set(null, "one");
            Assert.That(storage.Get<string>(null), Is.EqualTo("one"));
            Assert.That(storage.Get<string>(""), Is.EqualTo("one"));
            storage.Set(null, "two");
            Assert.That(storage.Get<string>(null), Is.EqualTo("two"));
            Assert.That(storage.Get<string>(""), Is.EqualTo("two"));
        }

        [Test]
        public void RecordDataPropertyReflectsChangesToFields() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            storage.Set(null, "one");
            storage.Set("alpha", "two");

            Assert.That(part.ContentItem.VersionRecord.Data, Is.EqualTo("<Data><Foo><Bar alpha=\"two\">one</Bar></Foo></Data>"));
        }

        [Test]
        public void ChangingRecordDataHasImmediateEffectOnStorageAccessors() {
            var part = CreateContentItemPart();
            var storage = _provider.BindStorage(part, part.PartDefinition.Fields.Single());

            storage.Set(null, "one");
            storage.Set("alpha", "two");

            Assert.That(part.ContentItem.VersionRecord.Data, Is.EqualTo("<Data><Foo><Bar alpha=\"two\">one</Bar></Foo></Data>"));
            part.ContentItem.VersionRecord.Data = "<Data><Foo><Bar alpha=\"four\">three</Bar></Foo></Data>";

            storage.Set(null, "three");
            storage.Set("alpha", "four");
        }

        [Test, Ignore("implementation pending")]
        public void VersionedSettingOnInfosetField() {
            Assert.Fail("todo");
        }
    }
}
