using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Fields;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Projections.Handlers;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Tests;
using Orchard.Tests.Stubs;
using Orchard.UI.PageClass;

namespace Orchard.Projections.Tests.Services {
    public class FieldIndexStorageTests : DatabaseEnabledTestsBase {
        private IFieldStorageProvider _provider;
        private IFieldIndexService _fieldIndexService;
        private IContentManager _contentManager;
        private IEnumerable<IFieldStorageEvents> _events;
        private IFieldStorage _storage;
        private ContentPart _part;
        private ContentItem _contentItem;

        public override void Register(ContainerBuilder builder) {
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<FieldIndexPartHandler>().As<IContentHandler>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<FieldIndexService>().As<IFieldIndexService>();

            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<FieldIndexPartHandler>().As<IContentHandler>();

            // ContentDefinitionManager
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IPageClassBuilder>().Object); 
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<FieldDriverStub>().As<IContentFieldDriver>();
            builder.RegisterType(typeof(SettingsFormatter)).As<ISettingsFormatter>();

            builder.RegisterType<InfosetStorageProvider>().As<IFieldStorageProvider>();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
        }

        public override void Init() {
            base.Init();

            _fieldIndexService = _container.Resolve<IFieldIndexService>();
            _contentManager = _container.Resolve<IContentManager>();
            _provider = _container.Resolve<IFieldStorageProvider>();
            _events = _container.Resolve<IEnumerable<IFieldStorageEvents>>();

            _part = CreateContentItemPart();
            var partFieldDefinition = _part.PartDefinition.Fields.Single();
            var storage = _provider.BindStorage(_part, partFieldDefinition);
            _storage = new FieldStorageEventStorage(storage, partFieldDefinition, _part, _events);
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] {
                    typeof(ContentItemRecord), 
                    typeof(ContentItemVersionRecord), 
                    typeof(ContentTypeRecord), 
                    
                    typeof(FieldIndexPartRecord), 
                    
                    typeof(StringFieldIndexRecord), 
                    typeof(IntegerFieldIndexRecord), 
                    typeof(DecimalFieldIndexRecord), 
                    typeof(DoubleFieldIndexRecord)
                };
            }
        }

        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>("thing"));
                Filters.Add(new ActivatingFilter<FieldIndexPart>("thing"));
            }
        }

        public class Thing : ContentPart {
        }

        private ContentPartDefinition FooPartDefinition() {
            return new ContentPartDefinitionBuilder()
                .Named("Foo")
                .WithField("Bar", cfg => cfg.OfType("TextField"))
                .Build();
        }

        private ContentPart CreateContentItemPart() {
            var partDefinition = FooPartDefinition();
            var typeDefinition = new ContentTypeDefinitionBuilder()
                .WithPart(partDefinition, part => { })
                .Build();
            _contentItem = new ContentItem {
                VersionRecord = new ContentItemVersionRecord {
                    ContentItemRecord = new ContentItemRecord(),
                    Published = false
                }
            };
            
            var contentPart = new ContentPart {
                TypePartDefinition = typeDefinition.Parts.Single()
            };
            _contentItem.Weld(contentPart);
            _contentItem.Weld(new InfosetPart {
                Infoset = _contentItem.Record.Infoset,
                VersionInfoset = _contentItem.VersionRecord.Infoset
            });
            _contentItem.Weld(new FieldIndexPart {
                Record = new FieldIndexPartRecord()
            });
            return contentPart;
        }

        private T Get<T>(string name) {
            return _fieldIndexService.Get<T>(_part.As<FieldIndexPart>(), "Foo", "Bar", name);
        }

        private void Set<T>(string name, T value) {
            _storage.Set(name, value);
        }

        [Test]
        public void ValueThatIsSetIsIndexed() {
            Set("alpha", "one");
            _contentManager.Publish(_contentItem);

            var indexedValue = Get<string>("alpha");
            Assert.That(indexedValue, Is.EqualTo("one"));
        }

        [Test]
        public void NullValueNamesShouldBeHandled() {
            Set(null, "one");
            _contentManager.Publish(_contentItem);

            var indexedValue = Get<string>(null);
            Assert.That(indexedValue, Is.EqualTo("one"));
        }

        [Test]
        public void CommonDataTypesShouldBeSerialized() {
            var datetime = new DateTime(1980, 1, 1, 12, 1, 1, 499);

            Set("string", "one");
            Set("int1", int.MaxValue);
            Set("int2", int.MinValue);
            Set("long1", long.MaxValue);
            Set("long2", long.MinValue);
            Set("datetime", datetime);
            Set("bool1", true);
            Set("bool2", false);
            Set("decimal1", decimal.MaxValue);
            Set("decimal2", decimal.MinValue);
            Set("double1", (double)123456.123456);
            Set("double2", (double)-123456.123456);

            _contentManager.Publish(_contentItem);

            Assert.That(Get<string>("string"), Is.EqualTo("one"));
            Assert.That(Get<int>("int1"), Is.EqualTo(int.MaxValue));
            Assert.That(Get<int>("int2"), Is.EqualTo(int.MinValue));
            Assert.That(Get<long>("long1"), Is.EqualTo(long.MaxValue));
            Assert.That(Get<long>("long2"), Is.EqualTo(long.MinValue));
            Assert.That(Get<DateTime>("datetime"), Is.EqualTo(datetime));
            Assert.That(Get<bool>("bool1"), Is.EqualTo(true));
            Assert.That(Get<bool>("bool2"), Is.EqualTo(false));
            Assert.That(Get<decimal>("decimal1"), Is.EqualTo(decimal.MaxValue));
            Assert.That(Get<decimal>("decimal2"), Is.EqualTo(decimal.MinValue));
            Assert.That(Get<double>("double1") - (double)123456.123456, Is.LessThan(1));
            Assert.That(Get<double>("double2") - (double)123456.123456, Is.LessThan(1));
        }

        [Test]
        public void StringsShouldBeTruncated() {

            Set("string", new string('x', 8000));

            _contentManager.Publish(_contentItem);

            Assert.That(Get<string>("string"), Is.EqualTo(new String('x', 4000)));
        }
    }

    public class FieldDriverStub : ContentFieldDriver<TextField> {
        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(string), null, null)
                .Member("alpha", typeof(string), null, null)
                .Member("string", typeof(string), null, null)
                .Member("int1", typeof(int), null, null)
                .Member("int2", typeof(int), null, null)
                .Member("long1", typeof(long), null, null)
                .Member("long2", typeof(long), null, null)
                .Member("datetime", typeof(DateTime), null, null)
                .Member("bool1", typeof(bool), null, null)
                .Member("bool2", typeof(bool), null, null)
                .Member("decimal1", typeof(decimal), null, null)
                .Member("decimal2", typeof(decimal), null, null)
                .Member("double1", typeof(double), null, null)
                .Member("double2", typeof(double), null, null);
        }
   
    }

}
