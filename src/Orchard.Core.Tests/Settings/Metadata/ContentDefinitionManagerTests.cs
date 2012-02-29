using System;
using System.IO;
using System.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.Core.Settings.Metadata;
using Orchard.Core.Settings.Metadata.Records;
using Orchard.Data;
using Orchard.Tests;
using Orchard.Tests.Stubs;
using Orchard.Tests.Utility;

namespace Orchard.Core.Tests.Settings.Metadata {
    [TestFixture]
    public class ContentDefinitionManagerTests {
        private string _databaseFileName;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private IContainer _container;

        [TestFixtureSetUp]
        public void InitFixture() {
            _databaseFileName = Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                _databaseFileName,
                typeof(ContentTypeDefinitionRecord),
                typeof(ContentTypePartDefinitionRecord),
                typeof(ContentPartDefinitionRecord),
                typeof(ContentPartFieldDefinitionRecord),
                typeof(ContentFieldDefinitionRecord)
                );
        }

        [SetUp]
        public void Init() {
            var builder = new ContainerBuilder();
            builder.RegisterAutoMocking();
            builder.RegisterType<ContentDefinitionManager>().As<IContentDefinitionManager>();
            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            builder.RegisterType(typeof(SettingsFormatter)).As(typeof(ISettingsFormatter));
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubParallelCacheContext>().As<IParallelCacheContext>();

            _container = builder.Build();

            _container.Mock<ISessionLocator>()
                .Setup(x => x.For(It.IsAny<Type>()))
                .Returns(() => _session);

            _session = _sessionFactory.OpenSession();
            foreach (var killType in new[] { typeof(ContentTypeDefinitionRecord), typeof(ContentPartDefinitionRecord), typeof(ContentFieldDefinitionRecord) }) {
                foreach (var killRecord in _session.CreateCriteria(killType).List()) {
                    _session.Delete(killRecord);
                }
            }
            _session.Flush();
        }

        void ResetSession() {
            _session.Flush();
            _session.Dispose();
            _session = _sessionFactory.OpenSession();
        }

        [TearDown]
        public void Term() {
            _session.Dispose();
        }

        [TestFixtureTearDown]
        public void TermFixture() {
            File.Delete(_databaseFileName);
        }

        [Test]
        public void NoTypesAreAvailableByDefault() {
            var types = _container.Resolve<IContentDefinitionManager>().ListTypeDefinitions();
            Assert.That(types.Count(), Is.EqualTo(0));
        }

        [Test]
        public void TypeRecordsAreReturned() {
            var repository = _container.Resolve<IRepository<ContentTypeDefinitionRecord>>();
            repository.Create(new ContentTypeDefinitionRecord { Name = "alpha" });
            repository.Create(new ContentTypeDefinitionRecord { Name = "beta" });
            ResetSession();
            var types = _container.Resolve<IContentDefinitionManager>().ListTypeDefinitions();
            Assert.That(types.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TypeSettingsAreParsed() {
            var repository = _container.Resolve<IRepository<ContentTypeDefinitionRecord>>();
            repository.Create(new ContentTypeDefinitionRecord { Name = "alpha", Settings = "<settings a='1' b='2'/>" });
            ResetSession();
            var alpha = _container.Resolve<IContentDefinitionManager>().ListTypeDefinitions().Single();
            Assert.That(alpha.Settings["a"], Is.EqualTo("1"));
            Assert.That(alpha.Settings["b"], Is.EqualTo("2"));
        }

        [Test]
        public void ContentTypesWithSettingsCanBeCreatedAndModified() {
            var manager = _container.Resolve<IContentDefinitionManager>();
            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder()
                                  .Named("alpha")
                                  .WithSetting("a", "1")
                                  .WithSetting("b", "2")
                                  .Build());

            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder()
                                  .Named("beta")
                                  .WithSetting("c", "3")
                                  .WithSetting("d", "4")
                                  .Build());

            ResetSession();

            var types1 = manager.ListTypeDefinitions();
            Assert.That(types1.Count(), Is.EqualTo(2));
            var alpha1 = types1.Single(t => t.Name == "alpha");
            Assert.That(alpha1.Settings["a"], Is.EqualTo("1"));
            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder(alpha1).WithSetting("a", "5").Build());
            ResetSession();

            var types2 = manager.ListTypeDefinitions();
            Assert.That(types2.Count(), Is.EqualTo(2));
            var alpha2 = types2.Single(t => t.Name == "alpha");
            Assert.That(alpha2.Settings["a"], Is.EqualTo("5"));
            Assert.That(alpha2.Settings["a"], Is.EqualTo("5"));
        }

        [Test]
        public void StubPartDefinitionsAreCreatedWhenContentTypesAreStored() {
            var manager = _container.Resolve<IContentDefinitionManager>();
            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder()
                                  .Named("alpha")
                                  .WithPart("foo", pb => { })
                                  .Build());

            ResetSession();

            var fooRecord = _container.Resolve<IRepository<ContentPartDefinitionRecord>>().Fetch(r => r.Name == "foo").SingleOrDefault();
            Assert.That(fooRecord, Is.Not.Null);
            Assert.That(fooRecord.Name, Is.EqualTo("foo"));

            var foo = manager.GetPartDefinition("foo");
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.Name, Is.EqualTo("foo"));

            var alpha = manager.GetTypeDefinition("alpha");
            Assert.That(alpha, Is.Not.Null);
            Assert.That(alpha.Parts.Count(), Is.EqualTo(1));
            Assert.That(alpha.Parts.Single().PartDefinition.Name, Is.EqualTo("foo"));
        }

        [Test]
        public void GettingDefinitionsByNameCanReturnNullAndWillAcceptNullEmptyOrInvalidNames() {
            var manager = _container.Resolve<IContentDefinitionManager>();
            Assert.That(manager.GetTypeDefinition("no such name"), Is.Null);
            Assert.That(manager.GetTypeDefinition(string.Empty), Is.Null);
            Assert.That(manager.GetTypeDefinition(null), Is.Null);
            Assert.That(manager.GetPartDefinition("no such name"), Is.Null);
            Assert.That(manager.GetPartDefinition(string.Empty), Is.Null);
            Assert.That(manager.GetPartDefinition(null), Is.Null);
        }

        [Test]
        public void PartsAreRemovedWhenNotReferencedButPartDefinitionRemains() {
            var manager = _container.Resolve<IContentDefinitionManager>();
            manager.StoreTypeDefinition(
                new ContentTypeDefinitionBuilder()
                    .Named("alpha")
                    .WithPart("foo", pb => { })
                    .WithPart("bar", pb => { })
                    .Build());

            AssertThatTypeHasParts("alpha","foo","bar");
            Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(2));
            ResetSession();
            AssertThatTypeHasParts("alpha","foo","bar");
            Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(2));

            manager.StoreTypeDefinition(
                new ContentTypeDefinitionBuilder(manager.GetTypeDefinition("alpha"))
                    .WithPart("frap", pb => { })
                    .RemovePart("bar")
                    .Build());

            AssertThatTypeHasParts("alpha","foo","frap");
            Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(3));
            ResetSession();
            AssertThatTypeHasParts("alpha","foo","frap");
            Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(3));
        }

        [Test]
        public void PartsCanBeDeleted() {
            var manager = _container.Resolve<IContentDefinitionManager>();
            manager.StoreTypeDefinition(
                new ContentTypeDefinitionBuilder()
                    .Named("alpha")
                    .WithPart("foo", pb => { })
                    .WithPart("bar", pb => { })
                    .Build());

            AssertThatTypeHasParts("alpha", "foo", "bar");
            Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(2));

            manager.DeletePartDefinition("foo");
            ResetSession();

            AssertThatTypeHasParts("alpha", "bar");
            Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(1));
        }

        [Test]
        public void ContentTypesCanBeDeleted() {
            var manager = _container.Resolve<IContentDefinitionManager>();
            manager.StoreTypeDefinition(
                new ContentTypeDefinitionBuilder()
                    .Named("alpha")
                    .WithPart("foo", pb => { })
                    .WithPart("bar", pb => { })
                    .Build());

            Assert.That(manager.GetTypeDefinition("alpha"), Is.Not.Null);
            manager.DeleteTypeDefinition("alpha");
            ResetSession();

            Assert.That(manager.GetTypeDefinition("alpha"), Is.Null);
        }

        private void AssertThatTypeHasParts(string typeName, params string[] partNames) {
            var type = _container.Resolve<IContentDefinitionManager>().GetTypeDefinition(typeName);
            Assert.That(type, Is.Not.Null);
            Assert.That(type.Parts.Count(), Is.EqualTo(partNames.Count()));
            foreach(var partName in partNames) {
                Assert.That(type.Parts.Select(p=>p.PartDefinition.Name), Has.Some.EqualTo(partName));
            }
        }
    }
}
