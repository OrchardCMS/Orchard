using System;
using System.Collections.Generic;
using Autofac;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.ContentManagement.Records;
using Orchard.Core.Settings.Metadata;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Projections.Handlers;
using Orchard.Projections.Models;
using Orchard.Projections.Services;
using Orchard.Tests;
using Orchard.Tests.Stubs;

namespace Orchard.Projections.Tests.Services {
    [TestFixture]
    public class FieldIndexServiceTests : DatabaseEnabledTestsBase {
        private IFieldIndexService _service;
        private IContentManager _contentManager;

        public override void Register(ContainerBuilder builder) {

            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<FieldIndexPartHandler>().As<IContentHandler>();
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<FieldIndexService>().As<IFieldIndexService>();

            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<FieldIndexPartHandler>().As<IContentHandler>();
            builder.RegisterType<InfosetStorageProvider>().As<IFieldStorageProvider>();

            // ContentDefinitionManager
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType(typeof(SettingsFormatter)).As<ISettingsFormatter>();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
        }

        public override void Init() {
            base.Init();

            _service = _container.Resolve<IFieldIndexService>();
            _contentManager = _container.Resolve<IContentManager>();
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

        [Test]
        public void StringValuesShouldBePersisted() {
            SaveObject<string>(null);
            SaveObject("");
            SaveObject("Bar");
        }

        [Test]
        public void IntegerValuesShouldBePersisted() {
            SaveObject<int?>(null);
            SaveObject(0);
            SaveObject(-42);
            SaveObject(42);
            SaveObject(long.MaxValue);
            SaveObject(long.MinValue);
        }

        [Test]
        public void DateTimeValuesShouldBePersisted() {
            SaveObject<DateTime?>(null);
            SaveObject(DateTime.MinValue);
            SaveObject(DateTime.MaxValue);
            SaveObject(DateTime.Now);
        }

        [Test]
        public void BooleanValuesShouldBePersisted() {
            SaveObject<bool?>(null);
            SaveObject(true);
            SaveObject(false);
        }

        [Test]
        public void DecimalValuesShouldBePersisted() {
            SaveObject<decimal?>(null);
            SaveObject(0m);
            SaveObject(-42m);
            SaveObject(42m);
            // SaveObject(decimal.MaxValue);
            // SaveObject(decimal.MinValue);
        }

        [Test, Ignore("SqlCe exception")]
        public void EdgeDecimalValuesShouldBePersisted() {
            SaveObject(decimal.MaxValue);
            SaveObject(decimal.MinValue);
        }

        [Test]
        public void DoubleValuesShouldBePersisted() {
            SaveObject<double?>(null);
            SaveObject(0D);
            SaveObject(-42D);
            SaveObject(42D);
            SaveObject(double.MaxValue);
            SaveObject(double.MinValue);
        }

        [Test]
        public void FloatValuesShouldBePersisted() {
            SaveObject<float?>(null);
            SaveObject(0F);
            SaveObject(-42F);
            SaveObject(42F);
            SaveObject(float.MaxValue);
            SaveObject(float.MinValue);
        }

        [Test]
        public void CharValuesShouldBePersisted() {
            SaveObject<char?>(null);
            SaveObject('a');
            SaveObject('ê');
        }

        private void SaveObject<T>(T fieldValue) {
            var thing = _contentManager.New("thing");
            _contentManager.Create(thing);

            _service.Set(thing.As<FieldIndexPart>(), "Foo", "Bar", "Baz", fieldValue, typeof(T));

            ClearSession();

            var thing2 = _contentManager.Get(thing.Id);
            var value = _service.Get<T>(thing2.As<FieldIndexPart>(), "Foo", "Bar", "Baz");

            Assert.That(value, Is.EqualTo(fieldValue));
        }

        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>("thing"));
                Filters.Add(new ActivatingFilter<FieldIndexPart>("thing"));
            }
        }

        public class Thing : ContentPart {
        }
    }
}
