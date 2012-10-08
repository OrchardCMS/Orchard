using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Moq;
using NHibernate;
using NUnit.Framework;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;
using Orchard.Tests.ContentManagement.Handlers;
using Orchard.Tests.ContentManagement.Records;
using Orchard.Tests.ContentManagement.Models;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement;
using System.Collections.Generic;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.ContentManagement {
    [TestFixture]
    public class DefaultContentManagerTests {
        private const string DefaultAlphaName = "alpha";
        private const string DefaultBetaName = "beta";
        private const string DefaultGammaName = "gamma";
        private const string DefaultDeltaName = "delta";

        private IContainer _container;
        private IContentManager _manager;
        private ISessionFactory _sessionFactory;
        private ISession _session;
        private Mock<IContentDefinitionManager> _contentDefinitionManager;

        [TestFixtureSetUp]
        public void InitFixture() {
            var databaseFileName = System.IO.Path.GetTempFileName();
            _sessionFactory = DataUtility.CreateSessionFactory(
                databaseFileName,
                typeof(ContentTypeRecord),
                typeof(ContentItemRecord),
                typeof(ContentItemVersionRecord),
                typeof(GammaRecord),
                typeof(DeltaRecord),
                typeof(EpsilonRecord),
                typeof(MegaRecord));
        }

        [SetUp]
        public void Init() {
            _contentDefinitionManager = new Mock<IContentDefinitionManager>();

            var builder = new ContainerBuilder();
            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(_contentDefinitionManager.Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);

            builder.RegisterType<AlphaPartHandler>().As<IContentHandler>();
            builder.RegisterType<BetaPartHandler>().As<IContentHandler>();
            builder.RegisterType<GammaPartHandler>().As<IContentHandler>();
            builder.RegisterType<DeltaPartHandler>().As<IContentHandler>();
            builder.RegisterType<EpsilonPartHandler>().As<IContentHandler>();
            builder.RegisterType<FlavoredPartHandler>().As<IContentHandler>();
            builder.RegisterType<StyledHandler>().As<IContentHandler>();
            builder.RegisterType<DefaultShapeTableManager>().As<IShapeTableManager>();
            builder.RegisterType<ShapeTableLocator>().As<IShapeTableLocator>();
            builder.RegisterType<DefaultShapeFactory>().As<IShapeFactory>();
            builder.RegisterType<DefaultContentDisplay>().As<IContentDisplay>();

            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));

            _session = _sessionFactory.OpenSession();
            builder.RegisterInstance(new TestSessionLocator(_session)).As<ISessionLocator>();

            _container = builder.Build();
            _manager = _container.Resolve<IContentManager>();
        }

        public class TestSessionLocator : ISessionLocator {
            private readonly ISession _session;

            public TestSessionLocator(ISession session) {
                _session = session;
            }

            public ISession For(Type entityType) {
                return _session;
            }
        }

        [Test]
        public void AlphaDriverShouldWeldItsPart() {
            var foo = _manager.New(DefaultAlphaName);

            Assert.That(foo.Is<AlphaPart>(), Is.True);
            Assert.That(foo.As<AlphaPart>(), Is.Not.Null);
            Assert.That(foo.Is<BetaPart>(), Is.False);
            Assert.That(foo.As<BetaPart>(), Is.Null);
        }

        [Test]
        public void StronglyTypedNewShouldTypeCast() {
            var foo = _manager.New<AlphaPart>(DefaultAlphaName);
            Assert.That(foo, Is.Not.Null);
            Assert.That(foo.GetType(), Is.EqualTo(typeof(AlphaPart)));
        }

        [Test, ExpectedException(typeof(InvalidCastException))]
        public void StronglyTypedNewShouldThrowCastExceptionIfNull() {
            _manager.New<BetaPart>(DefaultAlphaName);
        }

        [Test]
        public void AlphaIsFlavoredAndStyledAndBetaIsFlavoredOnly() {
            var alpha = _manager.New<AlphaPart>(DefaultAlphaName);
            var beta = _manager.New<BetaPart>(DefaultBetaName);

            Assert.That(alpha.Is<FlavoredPart>(), Is.True);
            Assert.That(alpha.Is<StyledPart>(), Is.True);
            Assert.That(beta.Is<FlavoredPart>(), Is.True);
            Assert.That(beta.Is<StyledPart>(), Is.False);
        }

        [Test]
        public void GetByIdShouldDetermineTypeAndLoadParts() {
            var modelRecord = CreateModelRecord(DefaultAlphaName);

            var contentItem = _manager.Get(modelRecord.Id);
            Assert.That(contentItem.ContentType, Is.EqualTo(DefaultAlphaName));
            Assert.That(contentItem.Id, Is.EqualTo(modelRecord.Id));
        }


        [Test]
        public void ModelPartWithRecordShouldCallRepositoryToPopulate() {

            CreateModelRecord(DefaultGammaName);
            CreateModelRecord(DefaultGammaName);
            var modelRecord = CreateModelRecord(DefaultGammaName);

            var model = _manager.Get(modelRecord.Id);

            //// create a gamma record
            //var gamma = new GammaRecord {
            //    ContentItemRecord = _container.Resolve<IRepository<ContentItemRecord>>().Get(model.Id),
            //    Frap = "foo"
            //};

            //_container.Resolve<IRepository<GammaRecord>>().Create(gamma);
            //_session.Flush();
            //_session.Clear();

            // re-fetch from database
            model = _manager.Get(modelRecord.Id);

            Assert.That(model.ContentType, Is.EqualTo(DefaultGammaName));
            Assert.That(model.Id, Is.EqualTo(modelRecord.Id));
            Assert.That(model.Is<GammaPart>(), Is.True);
            Assert.That(model.As<GammaPart>().Record, Is.Not.Null);
            Assert.That(model.As<GammaPart>().Record.ContentItemRecord.Id, Is.EqualTo(model.Id));

        }

        [Test]
        public void CreateShouldMakeModelAndContentTypeRecords() {
            var beta = _manager.New(DefaultBetaName);
            _manager.Create(beta);

            var modelRecord = _container.Resolve<IRepository<ContentItemRecord>>().Get(beta.Id);
            Assert.That(modelRecord, Is.Not.Null);
            Assert.That(modelRecord.ContentType.Name, Is.EqualTo(DefaultBetaName));
        }

        /// <summary>
        /// Tests that the GetContentTypeDefinitions returns only the registered types.
        /// </summary>
        [Test]
        public void GetContentTypesShouldReturnAllTypes() {
            // Register the types and obtain them
            ContentTypeDefinition alphaType = new ContentTypeDefinitionBuilder()
                .Named(DefaultAlphaName)
                .Build();

            ContentTypeDefinition betaType = new ContentTypeDefinitionBuilder()
                .Named(DefaultBetaName)
                .Build();

            ContentTypeDefinition gammaType = new ContentTypeDefinitionBuilder()
                .Named(DefaultGammaName)
                .Build();

            ContentTypeDefinition deltaType = new ContentTypeDefinitionBuilder()
                .Named(DefaultDeltaName)
                .Build();

            _contentDefinitionManager.Setup(contentDefinitionManager => contentDefinitionManager.ListTypeDefinitions())
                .Returns(new List<ContentTypeDefinition> { alphaType, betaType, gammaType, deltaType });
            
            var types = _manager.GetContentTypeDefinitions();

            // Validate that the expected types were obtained
            Assert.That(types.Count(), Is.EqualTo(4));
            Assert.That(types, Has.Some.With.Property("Name").EqualTo(DefaultAlphaName));
            Assert.That(types, Has.Some.With.Property("Name").EqualTo(DefaultBetaName));
            Assert.That(types, Has.Some.With.Property("Name").EqualTo(DefaultGammaName));
            Assert.That(types, Has.Some.With.Property("Name").EqualTo(DefaultDeltaName));
        }

        [Test]
        public void BigStringsShouldNotBeTruncated() {
            var megaRepository = _container.Resolve<IRepository<MegaRecord>>();
            var mega = new MegaRecord() { BigStuff = new string('x', 20000) };
            megaRepository.Create(mega);
            _session.Flush();
        }

        [Test, ExpectedException]
        public void StandardStringsShouldNotHaveAStandardSize() {
            var megaRepository = _container.Resolve<IRepository<MegaRecord>>();
            var mega = new MegaRecord() { SmallStuff = new string('x', 256) };
            megaRepository.Create(mega);
            _session.Flush();
        }

        private ContentItemRecord CreateModelRecord(string contentType) {
            var contentTypeRepository = _container.Resolve<IRepository<ContentTypeRecord>>();
            var contentItemRepository = _container.Resolve<IRepository<ContentItemRecord>>();
            var contentItemVersionRepository = _container.Resolve<IRepository<ContentItemVersionRecord>>();

            var modelRecord = new ContentItemRecord { ContentType = contentTypeRepository.Get(x => x.Name == contentType) };
            if (modelRecord.ContentType == null) {
                modelRecord.ContentType = new ContentTypeRecord { Name = contentType };
                contentTypeRepository.Create(modelRecord.ContentType);
            }
            contentItemRepository.Create(modelRecord);

            contentItemVersionRepository.Create(new ContentItemVersionRecord { ContentItemRecord = modelRecord, Latest = true, Published = true, Number = 1 });

            _session.Flush();
            _session.Clear();
            return modelRecord;
        }

        [Test]
        public void InitialVersionShouldBeOne() {
            var gamma1 = _manager.Create<GammaPart>(DefaultGammaName);
            Assert.That(gamma1.ContentItem.Record, Is.Not.Null);
            Assert.That(gamma1.ContentItem.VersionRecord, Is.Not.Null);
            Assert.That(gamma1.ContentItem.Version, Is.EqualTo(1));
            Assert.That(gamma1.ContentItem.VersionRecord.Number, Is.EqualTo(1));

            _session.Flush();
            _session.Clear();
            Trace.WriteLine("session flushed");

            var gamma2 = _manager.Get<GammaPart>(gamma1.ContentItem.Id);
            Assert.That(gamma2.ContentItem.Record, Is.Not.Null);
            Assert.That(gamma2.ContentItem.VersionRecord, Is.Not.Null);
            Assert.That(gamma2.ContentItem.Version, Is.EqualTo(1));
            Assert.That(gamma2.ContentItem.VersionRecord.Number, Is.EqualTo(1));

            // asserts results are re-acquired from db
            Assert.That(gamma1, Is.Not.SameAs(gamma2));
            Assert.That(gamma1.Record, Is.Not.SameAs(gamma2.Record));
            Assert.That(gamma1.ContentItem, Is.Not.SameAs(gamma2.ContentItem));
            Assert.That(gamma1.ContentItem.Record, Is.Not.SameAs(gamma2.ContentItem.Record));
            Assert.That(gamma1.ContentItem.VersionRecord, Is.Not.SameAs(gamma2.ContentItem.VersionRecord));
        }

        [Test]
        public void InitialVersionCanBeSpecifiedAndIsPublished() {
            var gamma1 = _manager.Create<GammaPart>(DefaultGammaName, VersionOptions.Number(4));

            Assert.That(gamma1.ContentItem.Version, Is.EqualTo(4));
            Assert.That(gamma1.ContentItem.VersionRecord.Published, Is.True);

            _session.Flush();
            _session.Clear();
        }

        [Test]
        public void PublishedShouldBeLatestButNotDraft() {
            var gamma1 = _manager.Create(DefaultGammaName, VersionOptions.Published);

            var gammaPublished = _manager.Get(gamma1.Id, VersionOptions.Published);
            var gammaLatest = _manager.Get(gamma1.Id, VersionOptions.Latest);
            var gammaDraft = _manager.Get(gamma1.Id, VersionOptions.Draft);

            Assert.That(gammaPublished.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaLatest.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaDraft, Is.Null);
        }

        [Test]
        public void DraftShouldBeLatestButNotPublished() {
            var gamma1 = _manager.Create(DefaultGammaName, VersionOptions.Draft);

            var gammaPublished = _manager.Get(gamma1.Id, VersionOptions.Published);
            var gammaLatest = _manager.Get(gamma1.Id, VersionOptions.Latest);
            var gammaDraft = _manager.Get(gamma1.Id, VersionOptions.Draft);

            Assert.That(gammaDraft.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaLatest.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaPublished, Is.Null);
        }


        [Test]
        public void CreateDraftShouldNotCreateExtraDraftCopies() {
            var gamma1 = _manager.Create(DefaultGammaName, VersionOptions.Draft);
            _session.Flush();
            _session.Clear();

            var gammaDraft1 = _manager.Get(gamma1.Id, VersionOptions.Draft);
            Assert.That(gammaDraft1.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaDraft1.Record.Versions, Has.Count.EqualTo(1));
            _session.Flush();
            _session.Clear();

            var gammaDraft2 = _manager.Get(gamma1.Id, VersionOptions.DraftRequired);
            Assert.That(gammaDraft2.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaDraft2.Record.Versions, Has.Count.EqualTo(1));
            _session.Flush();
            _session.Clear();

            var gammaDraft3 = _manager.Get(gamma1.Id, VersionOptions.Draft);
            Assert.That(gammaDraft3.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaDraft3.Record.Versions, Has.Count.EqualTo(1));
            _session.Flush();
            _session.Clear();

            var gammaDraft4 = _manager.Get(gamma1.Id, VersionOptions.DraftRequired);
            Assert.That(gammaDraft4.VersionRecord.Id, Is.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gammaDraft4.Record.Versions, Has.Count.EqualTo(1));
            _session.Flush();
            _session.Clear();
        }

        [Test]
        public void DraftRequiredShouldBuildNewVersionIfLatestIsAlreadyPublished() {
            Trace.WriteLine("gamma1");
            var gamma1 = _manager.Create(DefaultGammaName, VersionOptions.Published);
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft1");
            var gammaDraft1 = _manager.Get(gamma1.Id, VersionOptions.Draft);
            Assert.That(gammaDraft1, Is.Null);
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft2");
            var gammaDraft2 = _manager.Get(gamma1.Id, VersionOptions.DraftRequired);
            Assert.That(gammaDraft2.VersionRecord.Id, Is.Not.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gamma1.Version, Is.EqualTo(1));
            Assert.That(gammaDraft2.Version, Is.EqualTo(2));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            foreach (var x in _container.Resolve<IRepository<ContentItemVersionRecord>>().Fetch(x => x != null)) {
                Trace.WriteLine(string.Format("{0}/{1} #{2} published:{3} latest:{4}",
                    x.ContentItemRecord.Id,
                    x.Id,
                    x.Number,
                    x.Published,
                    x.Latest));
            }

            Trace.WriteLine("gammaDraft3");
            var gammaDraft3 = _manager.Get(gamma1.Id, VersionOptions.Draft);
            Assert.That(gammaDraft3.VersionRecord.Id, Is.EqualTo(gammaDraft2.VersionRecord.Id));
            Assert.That(gammaDraft3.Record, Is.Not.SameAs(gammaDraft2.Record));
            Assert.That(gammaDraft3.Record.Versions, Is.Not.SameAs(gammaDraft2.Record.Versions));

            Assert.That(gammaDraft3.Record.Versions, Has.Count.EqualTo(2));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft4");
            var gammaDraft4 = _manager.Get(gamma1.Id, VersionOptions.DraftRequired);
            Assert.That(gammaDraft4.VersionRecord.Id, Is.EqualTo(gammaDraft2.VersionRecord.Id));
            Assert.That(gammaDraft4.Record.Versions, Has.Count.EqualTo(2));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gamma2");
            var gamma2 = _manager.Get(gamma1.Id);
            Assert.That(gamma2.Record.Versions, Has.Count.EqualTo(2));
        }

        [Test]
        public void UsingGetManyDraftRequiredShouldBuildNewVersionIfLatestIsAlreadyPublished() {
            Trace.WriteLine("gamma1");
            var gamma1 = _manager.Create(DefaultGammaName, VersionOptions.Published);
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft1");
            var gammaDraft1 = _manager.GetMany<ContentItem>(new [] { gamma1.Id }, VersionOptions.Draft, QueryHints.Empty);
            Assert.That(gammaDraft1.Count(), Is.EqualTo(0));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft2");
            var gammaDraft2 = _manager.GetMany<ContentItem>(new[] { gamma1.Id }, VersionOptions.DraftRequired, QueryHints.Empty);
            Assert.That(gammaDraft2.Count(), Is.EqualTo(1));
            Assert.That(gammaDraft2.First().VersionRecord.Id, Is.Not.EqualTo(gamma1.VersionRecord.Id));
            Assert.That(gamma1.Version, Is.EqualTo(1));
            Assert.That(gammaDraft2.First().Version, Is.EqualTo(2));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();
        }


        [Test]
        public void UsingQueryDraftRequiredShouldBuildNewVersionIfLatestIsAlreadyPublished() {
            Trace.WriteLine("gamma1");
            var gamma1 = _manager.Create<GammaPart>(DefaultGammaName, VersionOptions.Published);
            gamma1.Record.Frap = "foo";
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaPublished");
            var gammaPublished = _manager.Query<GammaPart, GammaRecord>().Where(x => x.Frap == "foo").List();
            Assert.That(gammaPublished.Count(), Is.EqualTo(1));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft1");
            var gammaDraft1 = _manager.Query<GammaPart, GammaRecord>(VersionOptions.Draft).Where(x => x.Frap == "foo").List();
            Assert.That(gammaDraft1.Count(), Is.EqualTo(0));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gammaDraft2");
            var gammaDraft2 = _manager.Query<GammaPart, GammaRecord>(VersionOptions.DraftRequired).Where(x => x.Frap == "foo").List();
            Assert.That(gammaDraft2.Count(), Is.EqualTo(1));
            Assert.That(gammaDraft2.First().ContentItem.VersionRecord.Id, Is.Not.EqualTo(gamma1.ContentItem.VersionRecord.Id));
            Assert.That(gamma1.ContentItem.Version, Is.EqualTo(1));
            Assert.That(gammaDraft2.First().ContentItem.Version, Is.EqualTo(2));
            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();
        }

        [Test]
        public void NonVersionedPartsAreBoundToSameRecord() {
            Trace.WriteLine("gamma1");
            var gamma1 = _manager.Create<GammaPart>(DefaultGammaName, VersionOptions.Published, init => init.Record.Frap = "version one");
            Trace.WriteLine("gamma2");
            var gamma2 = _manager.Get<GammaPart>(gamma1.ContentItem.Id, VersionOptions.DraftRequired);
            Assert.That(gamma1.Record.Frap, Is.EqualTo("version one"));
            Assert.That(gamma2.Record.Frap, Is.EqualTo("version one"));
            gamma2.Record.Frap = "version two";
            Assert.That(gamma1.Record.Frap, Is.EqualTo("version two"));
            Assert.That(gamma2.Record.Frap, Is.EqualTo("version two"));

            Trace.WriteLine("flush");
            _session.Flush();
            _session.Clear();

            Trace.WriteLine("gamma1B");
            var gamma1B = _manager.Get<GammaPart>(gamma1.ContentItem.Id, VersionOptions.Published);
            Trace.WriteLine("gamma2B");
            var gamma2B = _manager.Get<GammaPart>(gamma1.ContentItem.Id, VersionOptions.Draft);
            Assert.That(gamma1B.Record, Is.SameAs(gamma2B.Record));
            Assert.That(gamma1B.Record.Frap, Is.EqualTo("version two"));
            Assert.That(gamma2B.Record.Frap, Is.EqualTo("version two"));
            Assert.That(gamma1B.ContentItem.VersionRecord.Id, Is.Not.EqualTo(gamma2B.ContentItem.VersionRecord.Id));

            Assert.That(gamma1.ContentItem.Record, Is.Not.SameAs(gamma1B.ContentItem.Record));
            Assert.That(gamma2.ContentItem.Record, Is.Not.SameAs(gamma2B.ContentItem.Record));
            Assert.That(gamma1.ContentItem.Record, Is.SameAs(gamma2.ContentItem.Record));
            Assert.That(gamma1B.ContentItem.Record, Is.SameAs(gamma2B.ContentItem.Record));
            Assert.That(gamma1.ContentItem.VersionRecord, Is.Not.SameAs(gamma2.ContentItem.VersionRecord));
            Assert.That(gamma1B.ContentItem.VersionRecord, Is.Not.SameAs(gamma2B.ContentItem.VersionRecord));

            Trace.WriteLine("flush");
            _session.Flush();
        }

        [Test]
        public void VersionedPartsShouldBeDifferentRecordsWithClonedData() {
            var gamma1 = _manager.Create<GammaPart>(DefaultGammaName, VersionOptions.Published, init => init.Record.Frap = "version one");
            var epsilon1 = gamma1.As<EpsilonPart>();
            epsilon1.Record.Quad = "epsilon one";

            var gamma2 = _manager.Get<GammaPart>(gamma1.ContentItem.Id, VersionOptions.DraftRequired);
            var epsilon2 = gamma2.As<EpsilonPart>();

            Assert.That(epsilon1.Record.Quad, Is.EqualTo("epsilon one"));
            Assert.That(epsilon2.Record.Quad, Is.EqualTo("epsilon one"));
            epsilon2.Record.Quad = "epsilon two";
            Assert.That(epsilon1.Record.Quad, Is.EqualTo("epsilon one"));
            Assert.That(epsilon2.Record.Quad, Is.EqualTo("epsilon two"));


            _session.Flush();
            _session.Clear();

            var gamma1B = _manager.Get<GammaPart>(gamma1.ContentItem.Id, VersionOptions.Published);
            var epsilon1B = gamma1B.As<EpsilonPart>();
            var gamma2B = _manager.Get<GammaPart>(gamma1.ContentItem.Id, VersionOptions.Draft);
            var epsilon2B = gamma2B.As<EpsilonPart>();
            Assert.That(gamma1B.Record, Is.SameAs(gamma2B.Record));
            Assert.That(epsilon1B.Record, Is.Not.SameAs(epsilon2B.Record));
            Assert.That(epsilon1B.Record.Quad, Is.EqualTo("epsilon one"));
            Assert.That(epsilon2B.Record.Quad, Is.EqualTo("epsilon two"));
            Assert.That(epsilon1B.ContentItem.VersionRecord.Id, Is.Not.EqualTo(epsilon2B.ContentItem.VersionRecord.Id));

            Assert.That(epsilon1.ContentItem.Record, Is.Not.SameAs(epsilon1B.ContentItem.Record));
            Assert.That(epsilon2.ContentItem.Record, Is.Not.SameAs(epsilon2B.ContentItem.Record));
            Assert.That(epsilon1.ContentItem.Record, Is.SameAs(epsilon2.ContentItem.Record));
            Assert.That(epsilon1B.ContentItem.Record, Is.SameAs(epsilon2B.ContentItem.Record));
            Assert.That(epsilon1.ContentItem.VersionRecord, Is.Not.SameAs(epsilon2.ContentItem.VersionRecord));
            Assert.That(epsilon1B.ContentItem.VersionRecord, Is.Not.SameAs(epsilon2B.ContentItem.VersionRecord));
        }

        private void Flush() {
            Trace.WriteLine("flush");
            _session.Flush();

        }
        private void FlushAndClear() {
            Trace.WriteLine("flush");
            _session.Flush();
            Trace.WriteLine("clear");
            _session.Clear();
        }

        [Test]
        public void GetAllVersionsShouldReturnHistoryInOrder() {
            Trace.WriteLine("gamma1");
            var gamma1 = _manager.Create(DefaultGammaName, VersionOptions.Published);
            Flush();

            Trace.WriteLine("gamma2");
            var gamma2 = _manager.GetDraftRequired(gamma1.Id);
            Trace.WriteLine("publish");
            _manager.Publish(gamma2);
            Flush();

            Trace.WriteLine("gamma3");
            var gamma3 = _manager.GetDraftRequired(gamma1.Id);
            Trace.WriteLine("publish");
            _manager.Publish(gamma3);
            Flush();

            Trace.WriteLine("gamma4");
            var gamma4 = _manager.GetDraftRequired(gamma1.Id);
            Trace.WriteLine("publish");
            _manager.Publish(gamma2);
            FlushAndClear();

            Assert.That(gamma1.Version, Is.EqualTo(1));
            Assert.That(gamma2.Version, Is.EqualTo(2));
            Assert.That(gamma3.Version, Is.EqualTo(3));
            Assert.That(gamma4.Version, Is.EqualTo(4));

            var gammas = _manager.GetAllVersions(gamma1.Id).ToList();

            Assert.That(gammas[0].Version, Is.EqualTo(1));
            Assert.That(gammas[1].Version, Is.EqualTo(2));
            Assert.That(gammas[2].Version, Is.EqualTo(3));
            Assert.That(gammas[3].Version, Is.EqualTo(4));
        }

        [Test]
        public void EmptyTypeDefinitionShouldBeCreatedIfNotAlreadyDefined() {
            var contentItem = _manager.New("no-such-type");
            Assert.That(contentItem.ContentType, Is.EqualTo("no-such-type"));
            Assert.That(contentItem.TypeDefinition, Is.Not.Null);
            Assert.That(contentItem.TypeDefinition.Name, Is.EqualTo("no-such-type"));
            Assert.That(contentItem.TypeDefinition.Settings.Count(), Is.EqualTo(0));
            Assert.That(contentItem.TypeDefinition.Parts.Count(), Is.EqualTo(0));
        }


        [Test]
        public void ExistingTypeAndPartDefinitionShouldBeUsed() {
            var alphaType = new ContentTypeDefinitionBuilder()
                .Named(DefaultAlphaName)
                .WithSetting("x", "1")
                .WithPart("foo")
                .WithPart("FlavoredPart", part => part.WithSetting("spin", "clockwise"))
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition(DefaultAlphaName))
                .Returns(alphaType);

            var contentItem = _manager.New(DefaultAlphaName);
            Assert.That(contentItem.ContentType, Is.EqualTo(DefaultAlphaName));
            Assert.That(contentItem.TypeDefinition, Is.Not.Null);
            Assert.That(contentItem.TypeDefinition, Is.SameAs(alphaType));

            var flavored = contentItem.As<FlavoredPart>();
            Assert.That(flavored, Is.Not.Null);
            Assert.That(flavored.TypePartDefinition, Is.Not.Null);
            Assert.That(flavored.TypePartDefinition.Settings["spin"], Is.EqualTo("clockwise"));
        }

        [Test]
        public void FieldsCanBeWeldIntoParts() {
            var contentItem = _manager.New(DefaultAlphaName);
            var part = contentItem.As<FlavoredPart>();
            var field = new Phi();
            part.Weld(field);
            Assert.That(part.Has(typeof(Phi), "Phi"));
        }

        [Test]
        public void PartGetReturnsFieldWithName() {
            var contentItem = _manager.New(DefaultAlphaName);
            var part = contentItem.As<FlavoredPart>();
            var field = new Phi();
            part.Weld(field);
            var phi = part.Get(typeof(Phi), "Phi");
            var phi2 = part.Get(typeof(Phi), "Phi2");
            Assert.That(phi.Name, Is.EqualTo("Phi"));
            Assert.That(phi2, Is.Null);
        }
    }
}

