using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using Lucene.Services;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.AppData;
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
        private IIndexNotifierHandler _indexNotifier;
        private IContentManager _contentManager;
        private Mock<IContentDefinitionManager> _contentDefinitionManager;
        private StubLogger _logger;
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
            
            builder.RegisterType<LuceneIndexProvider>().As<IIndexProvider>();
            builder.RegisterInstance(_appDataFolder).As<IAppDataFolder>();
            builder.RegisterType<IndexingTaskExecutor>().As<IIndexNotifierHandler>();
            builder.RegisterType<DefaultIndexManager>().As<IIndexManager>();
            builder.RegisterType<IndexingTaskManager>().As<IIndexingTaskManager>();
            builder.RegisterType<IndexSynLock>().As<IIndexSynLock>();
            builder.RegisterType<DefaultContentManager>().As<IContentManager>();
            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(_contentDefinitionManager.Object);
            builder.RegisterInstance(new Mock<IContentDisplay>().Object);

            builder.RegisterType<DefaultContentManagerSession>().As<IContentManagerSession>();
            builder.RegisterInstance(new Mock<ITransactionManager>().Object);
            builder.RegisterInstance(new Mock<IAuthorizer>().Object);
            builder.RegisterType<OrchardServices>().As<IOrchardServices>();

            builder.RegisterType<ThingHandler>().As<IContentHandler>();
            builder.RegisterType<CreateIndexingTaskHandler>().As<IContentHandler>();

            builder.RegisterType<DefaultContentQuery>().As<IContentQuery>();
            builder.RegisterType<BodyPartHandler>().As<IContentHandler>();
            builder.RegisterType<StubExtensionManager>().As<IExtensionManager>();

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

            _provider = _container.Resolve<IIndexProvider>();
            _indexNotifier = _container.Resolve<IIndexNotifierHandler>();
            _contentManager = _container.Resolve<IContentManager>();
            ((IndexingTaskExecutor)_indexNotifier).Logger = _logger = new StubLogger();

            var thingType = new ContentTypeDefinitionBuilder()
                .Named(ThingDriver.ContentTypeName)
                .WithSetting("TypeIndexing.Included", "true")
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition(ThingDriver.ContentTypeName))
                .Returns(thingType);
        }

        private string[] Indexes() {
            return new DirectoryInfo(Path.Combine(_basePath, "Sites", "My Site", "Indexes")).GetDirectories().Select(d => d.Name).ToArray();
        }

        [Test]
        public void IndexShouldBeEmptyWhenThereIsNoContent() {
            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(0));
            Assert.That(_logger.LogEntries.Count(), Is.EqualTo(2));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Index update requested, nothing to do"));
        }

        [Test]
        public void ShouldIngoreNonIndexableContentWhenRebuildingTheIndex() {
            var alphaType = new ContentTypeDefinitionBuilder()
                .Named("alpha")
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition("alpha"))
                .Returns(alphaType);

            _contentManager.Create("alpha");

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(0));
            Assert.That(_logger.LogEntries.Count(), Is.EqualTo(2));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Index update requested, nothing to do"));
        }

        [Test]
        public void ShouldNotIndexContentIfIndexDocumentIsEmpty() {
            var alphaType = new ContentTypeDefinitionBuilder()
                .Named("alpha")
                .WithSetting("TypeIndexing.Included", "true") // the content types should be indexed, but there is no content at all
                .Build();

            _contentDefinitionManager
                .Setup(x => x.GetTypeDefinition("alpha"))
                .Returns(alphaType);

            _contentManager.Create("alpha");

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(0));
            Assert.That(_logger.LogEntries.Count(), Is.EqualTo(2));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Index update requested, nothing to do"));
        }

        [Test]
        public void ShouldIndexContentIfSettingsIsSetAndHandlerIsProvided() {
            var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
            content.Text = "Lorem ipsum";

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
            Assert.That(_logger.LogEntries.Count(), Is.EqualTo(3));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Processing {0} indexing tasks"));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Added content items to index: {0}"));
        }

        [Test]
        public void ShouldUpdateTheIndexWhenContentIsPublished() {
            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";
            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            _logger.Clear();

            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";
            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(2));
            Assert.That(_logger.LogEntries, Has.None.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
        }

        [Test]
        public void ShouldUpdateTheIndexWhenContentIsUnPublished() {
            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";
            _clock.Advance(TimeSpan.FromSeconds(1));

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            _logger.Clear();

            var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
            content.Text = "Lorem ipsum";
            _clock.Advance(TimeSpan.FromSeconds(1));
            
            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(2));
            Assert.That(_logger.LogEntries, Has.None.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            _clock.Advance(TimeSpan.FromSeconds(1));

            _contentManager.Unpublish(content.ContentItem);
            _clock.Advance(TimeSpan.FromSeconds(1));
            
            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
            Assert.That(_logger.LogEntries, Has.None.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
        }

        [Test]
        public void ShouldRemoveFromIndexEvenIfPublishedAndUnpublishedInTheSameSecond() {
            // This test is to ensure that when a task is created, all previous tasks for the same content item
            // are also removed, and thus that multiple tasks don't conflict while updating the index
            
            _contentManager.Create<Thing>(ThingDriver.ContentTypeName).Text = "Lorem ipsum";
            _clock.Advance(TimeSpan.FromSeconds(1));

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
            Assert.That(_logger.LogEntries, Has.Some.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
            _logger.Clear();

            var content = _contentManager.Create<Thing>(ThingDriver.ContentTypeName);
            content.Text = "Lorem ipsum";

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(2));
            Assert.That(_logger.LogEntries, Has.None.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));

            _contentManager.Unpublish(content.ContentItem);

            _indexNotifier.UpdateIndex(IndexName);
            Assert.That(_provider.NumDocs(IndexName), Is.EqualTo(1));
            Assert.That(_logger.LogEntries, Has.None.Matches<LogEntry>(entry => entry.LogFormat == "Rebuild index started"));
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
                LogEntries.Add(new LogEntry() {
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
