using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Lucene.Services;
using Moq;
using NUnit.Framework;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.LockFile;
using Orchard.Indexing;
using Orchard.Indexing.Handlers;
using Orchard.Indexing.Models;
using Orchard.Indexing.Services;
using Orchard.Logging;
using Orchard.Security;
using Orchard.Tasks.Indexing;
using Orchard.Tests.FileSystems.AppData;
using Orchard.Tests.Stubs;

namespace Orchard.Tests.Modules.Indexing {
    public class IndexingTaskExecutorTests : DatabaseEnabledTestsBase {
        private IIndexProvider _provider;
        private IAppDataFolder _appDataFolder;
        private ShellSettings _shellSettings;
        private IIndexingTaskExecutor _indexTaskExecutor;
        private IContentManager _contentManager;
        private Mock<IContentDefinitionManager> _contentDefinitionManager;
        private StubLogger _logger;
        private ILockFileManager _lockFileManager;

        private const string IndexName = "Search";
        private readonly string _basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        [TestFixtureTearDown]
        public void Clean() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
        }

        public override void Register(ContainerBuilder builder) {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }

            Directory.CreateDirectory(_basePath);
            _contentDefinitionManager = new Mock<IContentDefinitionManager>();
            _appDataFolder = AppDataFolderTests.CreateAppDataFolder(_basePath);

            builder.RegisterType<DefaultLuceneAnalyzerProvider>().As<ILuceneAnalyzerProvider>();
            builder.RegisterType<DefaultLuceneAnalyzerSelector>().As<ILuceneAnalyzerSelector>();
            builder.RegisterType<LuceneIndexProvider>().As<IIndexProvider>();
            builder.RegisterInstance(_appDataFolder).As<IAppDataFolder>();
            builder.RegisterType<IndexingTaskExecutor>().As<IIndexingTaskExecutor>();
            builder.RegisterType<DefaultIndexManager>().As<IIndexManager>();
            builder.RegisterType<IndexingTaskManager>().As<IIndexingTaskManager>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<StubCacheManager>().As<ICacheManager>();
            builder.RegisterType<StubWorkContextAccessor>().As<IWorkContextAccessor>();
            builder.RegisterType<Signals>().As<ISignals>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(_contentDefinitionManager.Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);

            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();

            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<CreateIndexingTaskHandler>().As<IContentHandler>();

            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<BodyPartHandler>().As<IContentHandler>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();

            builder.RegisterType<DefaultLockFileManager>().As<ILockFileManager>();

            // setting up a ShellSettings instance
            _shellSettings = new ShellSettings { Name = "My Site" };
            builder.RegisterInstance(_shellSettings).As<ShellSettings>();
        }

        protected override IEnumerable<Type> DatabaseTypes {
            get {
                return new[] { typeof(IndexingTaskRecord),
                    typeof(ContentTypeRecord),
                    typeof(ContentItemRecord),
                    typeof(ContentItemVersionRecord), 
                    typeof(BodyPartRecord), 
                    typeof(CommonPartRecord),
                    typeof(CommonPartVersionRecord),
                };
            }
        }

        public override void Init() {
            base.Init();
            _lockFileManager = _container.Resolve<ILockFileManager>();
            _provider = _container.Resolve<IIndexProvider>();
            _indexTaskExecutor = _container.Resolve<IIndexingTaskExecutor>();
            _contentManager = _container.Resolve<IContentManager>();
            ((IndexingTaskExecutor)_indexTaskExecutor).Logger = _logger = new StubLogger();

            var thingType = new ContentTypeDefinitionBuilder()
                .Named(ThingDriver.ContentTypeName)
                .WithSetting("TypeIndexing.Indexes", "Search")
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition(ThingDriver.ContentTypeName))
                .Returns(thingType);
        }

        [Test]
        public void IndexShouldBeEmptyWhenThereIsNoContent() {
            while(_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(0));
        }

        [Test]
        public void ShouldIgnoreNonIndexableContentWhenRebuildingTheIndex() {
            var alphaType = new ContentTypeDefinitionBuilder()
                .Named("alpha")
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition("alpha"))
                .Returns(alphaType);

            _contentManager.Create("alpha");

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(0));
        }

        [Test]
        public void ShouldNotIndexContentIfIndexDocumentIsEmpty() {
            var alphaType = new ContentTypeDefinitionBuilder()
                .Named("alpha")
                .WithSetting("TypeIndexing.Indexes", "Search") // the content types should be indexed, but there is no content at all
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition("alpha"))
                .Returns(alphaType);

            _contentManager.Create("alpha");

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(0));
        }

        [Test]
        public void ShouldIndexContentIfSettingsIsSetAndHandlerIsProvided() {
            var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
            content.Text = "Lorem ipsum";

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
        }

        [Test]
        public void ShouldUpdateTheIndexWhenContentIsPublished() {
            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";
            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));

            // there should be nothing done
            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));

            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";
            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(2));
        }

        [Test]
        public void IndexingTaskExecutorShouldNotBeReEntrant() {
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock("Sites/My Site/Search.settings.xml.lock", ref lockFile);
            using (lockFile) {
                while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
                Assert.That(_logger.LogEntries.Count, Is.EqualTo(1));
                Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Index was requested but is already running"));
            }

            _logger.LogEntries.Clear();
            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_logger.LogEntries, Has.None.Matches<LogEntry>(entry => entry.LogFormat == "Index was requested but is already running"));
        }

        [Test]
        public void ShouldUpdateTheIndexWhenContentIsUnPublished() {
            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));

            var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
            content.Text = "Lorem ipsum";

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(2));

            _contentManager.Unpublish(content.ContentItem);

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
        }

        [Test]
        public void ShouldUpdateTheIndexWhenContentIsDeleted() {
            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) { }
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));

            var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
            content.Text = "Lorem ipsum";

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) { }
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(2));

            _contentManager.Remove(content.ContentItem);

            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) { }
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
        }

        [Test]
        public void ShouldIndexAllContentOverTheLoopSize() {
            for (int i = 0; i < 999; i++) {
                var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
                content.Text = "Lorem ipsum " + i;
            }
            while (_indexTaskExecutor.UpdateIndexBatch(IndexName)) {}
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(999));
        }

        #region Stubs

        public class ThingHandler : ContentHandler {
            public ThingHandler() {
                Filters.Add(new ActivatingFilter<Thing>(ThingDriver.ContentTypeName));
                Filters.Add(new ActivatingFilter<ContentPart<CommonPartVersionRecord>>(ThingDriver.ContentTypeName));
                Filters.Add(new ActivatingFilter<CommonPart>(ThingDriver.ContentTypeName));
                Filters.Add(new ActivatingFilter<BodyPart>(ThingDriver.ContentTypeName));
            }
        }

        public class Thing : ContentPart {
            public string Text {
                get { return this.As<BodyPart>().Text; }
                set { this.As<BodyPart>().Text = value; }
            }
        }

        public class ThingDriver : ContentPartDriver<Thing> {
            public static readonly string ContentTypeName = "thing";
        }

        public class LogEntry {
            public Exception LogException { get; set; }
            public string LogFormat { get; set; }
            public object[] LogArgs { get; set; }
            public LogLevel LogLevel { get; set; }
        }

        public class StubLogger : ILogger {
            public List<LogEntry> LogEntries { get; set; }

            public StubLogger() {
                LogEntries = new List<LogEntry>();
            }

            public void Clear() {
                LogEntries.Clear();
            }

            public bool IsEnabled(LogLevel level) {
                return true;
            }

            public void Log(LogLevel level, Exception exception, string format, params object[] args) {
                LogEntries.Add(new LogEntry {
                    LogArgs = args,
                    LogException = exception,
                    LogFormat = format,
                    LogLevel = level
                });
            }
        }

        #endregion
    }
}
