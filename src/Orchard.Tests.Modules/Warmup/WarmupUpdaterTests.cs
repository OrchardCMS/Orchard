using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using Autofac;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Configuration;
using Orchard.Environment.Warmup;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.LockFile;
using Orchard.Services;
using Orchard.Tests.FileSystems.AppData;
using Orchard.Tests.Stubs;
using Orchard.Tests.UI.Navigation;
using Orchard.Warmup.Models;
using Orchard.Warmup.Services;

namespace Orchard.Tests.Modules.Warmup {
    public class WarmupUpdaterTests {
        protected IContainer _container;
        private IWarmupUpdater _warmupUpdater;
        private IAppDataFolder _appDataFolder;
        private ILockFileManager _lockFileManager;
        private StubClock _clock;
        private Mock<IWebDownloader> _webDownloader;
        private IOrchardServices _orchardServices;
        private WarmupSettingsPart _settings;
        private IWarmupReportManager _reportManager;

        private readonly string _basePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        private string _warmupFilename, _lockFilename;
        private const string WarmupFolder = "Warmup";
        private const string TenantFolder = "Sites/Default";

        [TestFixtureTearDown]
        public void Clean() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }
        }

        [SetUp]
        public void Init() {
            if (Directory.Exists(_basePath)) {
                Directory.Delete(_basePath, true);
            }

            Directory.CreateDirectory(_basePath);
            _appDataFolder = AppDataFolderTests.CreateAppDataFolder(_basePath);
            _webDownloader = new Mock<IWebDownloader>();
            _orchardServices = new StubOrchardServices();
            ((StubWorkContextAccessor.WorkContextImpl.StubSite) _orchardServices.WorkContext.CurrentSite).BaseUrl = "http://orchardproject.net";

            _settings = new WarmupSettingsPart();

            _orchardServices.WorkContext.CurrentSite.ContentItem.Weld(_settings);
            _orchardServices.WorkContext.CurrentSite.ContentItem.Weld(new InfosetPart());
 
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_appDataFolder).As<IAppDataFolder>();
            builder.RegisterInstance(_orchardServices).As<IOrchardServices>();
            builder.RegisterType<DefaultLockFileManager>().As<ILockFileManager>();
            builder.RegisterType<WarmupUpdater>().As<IWarmupUpdater>();
            builder.RegisterType<StubClock>().As<IClock>();
            builder.RegisterType<WarmupReportManager>().As<IWarmupReportManager>();
            builder.RegisterInstance(new ShellSettings { Name = "Default" }).As<ShellSettings>();
            builder.RegisterInstance(_clock = new StubClock()).As<IClock>();
            builder.RegisterInstance(_webDownloader.Object).As<IWebDownloader>();
            _container = builder.Build();

            _lockFileManager = _container.Resolve<ILockFileManager>();
            _warmupUpdater = _container.Resolve<IWarmupUpdater>();
            _reportManager = _container.Resolve<IWarmupReportManager>();

            _warmupFilename = _appDataFolder.Combine(TenantFolder, "warmup.txt");
            _lockFilename = _appDataFolder.Combine(TenantFolder, "warmup.txt.lock");
        }

        [Test]
        public void ShouldDoNothingWhenNoUrlsAreSpecified() {
            _warmupUpdater.EnsureGenerate();
            Assert.That(_appDataFolder.ListFiles(WarmupFolder).Count(), Is.EqualTo(0));
        }

        [Test]
        public void StampFileShouldBeDeletedToForceAnUpdate() {
            _appDataFolder.CreateFile(_warmupFilename, "");
            _warmupUpdater.Generate();
            Assert.That(_appDataFolder.ListFiles(WarmupFolder).Count(), Is.EqualTo(0));
        }

        [Test]
        public void GenerateShouldNotRunIfLocked() {
            _appDataFolder.CreateFile(_warmupFilename, "");
            ILockFile lockFile = null;
            _lockFileManager.TryAcquireLock(_lockFilename, ref lockFile);
            using(lockFile) {
                _warmupUpdater.Generate();
                Assert.That(_appDataFolder.ListFiles(WarmupFolder).Count(), Is.EqualTo(0));
            }

            _warmupUpdater.Generate();
            Assert.That(_appDataFolder.ListFiles(WarmupFolder).Count(), Is.EqualTo(0));
        }

        [Test]
        public void ShouldDownloadConfiguredUrls() {
            _settings.Urls = @" /
                                /About";
            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });
            
            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });
            
            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();
            
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));

            files = _appDataFolder.ListFiles(TenantFolder).ToList();

            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(TenantFolder, "warmup.txt")));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(TenantFolder, "warmup.xml")));

            var homepageContent = _appDataFolder.ReadFile(_appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net")));
            var aboutcontent = _appDataFolder.ReadFile(_appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About")));

            Assert.That(homepageContent, Is.EqualTo("Foo"));
            Assert.That(aboutcontent, Is.EqualTo("Bar"));
        }

        [Test]
        public void ShouldCreateFilesForOkStatusOnly() {
            _settings.Urls = @" /
                                /About";
            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });
            
            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.NotFound });

            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();

            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
            Assert.That(files, Has.None.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));
        }

        [Test]
        public void ShouldProcessValidRequestsOnly() {
            _settings.Urls = @" /
                                <>@\\";
            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
        }

        [Test]
        public void WarmupFileShouldContainUtcNow() {
            _settings.Urls = @"/";
            
            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _warmupUpdater.Generate();

            var warmupContent = _appDataFolder.ReadFile(_warmupFilename);
            Assert.That(warmupContent, Is.EqualTo(XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc)));
        }

        [Test]
        public void ShouldNotProcessIfDelayHasNotExpired() {
            _settings.Urls = @"/";
            _settings.Delay = 90;
            _settings.Scheduled = true;

            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });

            _warmupUpdater.Generate();

            var warmupContent = _appDataFolder.ReadFile(_warmupFilename);
            Assert.That(warmupContent, Is.EqualTo(XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc)));

            _settings.Urls = @" /
                                /About";
            _clock.Advance(TimeSpan.FromMinutes(89));
            _warmupUpdater.EnsureGenerate();
            
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();
            Assert.That(files, Has.None.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));
            
            warmupContent = _appDataFolder.ReadFile(_warmupFilename);
            Assert.That(warmupContent, Is.Not.EqualTo(XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc)));
        }

        [Test]
        public void ShouldProcessIfDelayHasExpired() {
            _settings.Urls = @"/";
            _settings.Delay = 90;
            _settings.Scheduled = true;

            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _webDownloader
                .Setup(w => w.Download("http://orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });

            _warmupUpdater.Generate();

            var warmupContent = _appDataFolder.ReadFile(_warmupFilename);
            Assert.That(warmupContent, Is.EqualTo(XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc)));

            _settings.Urls = @" /
                                /About";
            _clock.Advance(TimeSpan.FromMinutes(91));
            _warmupUpdater.EnsureGenerate();

            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));

            warmupContent = _appDataFolder.ReadFile(_warmupFilename); 
            Assert.That(warmupContent, Is.EqualTo(XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc)));
        }

        [Test]
        public void ShouldGenerateNonWwwVersions() {
            _settings.Urls = @" /
                                /About";

            ((StubWorkContextAccessor.WorkContextImpl.StubSite)_orchardServices.WorkContext.CurrentSite).BaseUrl = "http://www.orchardproject.net/";

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });
            
            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });
            
            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();
            
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net/About"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));

            var homepageContent = _appDataFolder.ReadFile(_appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net")));
            var aboutcontent = _appDataFolder.ReadFile(_appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About")));

            var wwwhomepageContent = _appDataFolder.ReadFile(_appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net")));
            var wwwaboutcontent = _appDataFolder.ReadFile(_appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net/About")));

            Assert.That(homepageContent, Is.EqualTo("Foo"));
            Assert.That(wwwhomepageContent, Is.EqualTo("Foo"));
            Assert.That(aboutcontent, Is.EqualTo("Bar"));
            Assert.That(wwwaboutcontent, Is.EqualTo("Bar"));
        }

        [Test]
        public void ReportIsCreated() {
            _settings.Urls = @" /
                                /About";

            ((StubWorkContextAccessor.WorkContextImpl.StubSite)_orchardServices.WorkContext.CurrentSite).BaseUrl = "http://www.orchardproject.net/";

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });

            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();

            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net/About"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));

            var report = _reportManager.Read().ToList();

            Assert.That(report.Count(), Is.EqualTo(2));
            Assert.That(report, Has.Some.Matches<ReportEntry>(x => x.RelativeUrl == "/"));
            Assert.That(report, Has.Some.Matches<ReportEntry>(x => x.RelativeUrl == "/About"));
        }

        [Test]
        public void ShouldNotDeleteOtherFiles() {
            _settings.Urls = @" /
                                /About";

            ((StubWorkContextAccessor.WorkContextImpl.StubSite)_orchardServices.WorkContext.CurrentSite).BaseUrl = "http://www.orchardproject.net/";

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });

            // Create a static file in the warmup folder
            _appDataFolder.CreateFile(_appDataFolder.Combine(WarmupFolder, "foo.txt"), "Foo");

            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();

            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, "foo.txt")));

            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net/About"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));
        }


        [Test]
        public void ClearingUrlsShouldDeleteContent() {
            _settings.Urls = @" /
                                /About";

            ((StubWorkContextAccessor.WorkContextImpl.StubSite)_orchardServices.WorkContext.CurrentSite).BaseUrl = "http://www.orchardproject.net/";

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/"))
                .Returns(new DownloadResult { Content = "Foo", StatusCode = HttpStatusCode.OK });

            _webDownloader
                .Setup(w => w.Download("http://www.orchardproject.net/About"))
                .Returns(new DownloadResult { Content = "Bar", StatusCode = HttpStatusCode.OK });

            _warmupUpdater.Generate();
            var files = _appDataFolder.ListFiles(WarmupFolder).ToList();

            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://www.orchardproject.net/About"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net"))));
            Assert.That(files, Has.Some.Matches<string>(x => x == _appDataFolder.Combine(WarmupFolder, WarmupUtility.EncodeUrl("http://orchardproject.net/About"))));

            _settings.Urls = @"";

            _warmupUpdater.Generate();
            files = _appDataFolder.ListFiles(WarmupFolder).ToList();

            Assert.That(files.Count, Is.EqualTo(0));
        }
    }
}
