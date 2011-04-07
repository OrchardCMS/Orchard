using System;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.Environment.Warmup;
using Orchard.FileSystems.AppData;
using Orchard.FileSystems.LockFile;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Services {
    public class WarmupUpdater : IWarmupUpdater {
        private readonly IOrchardServices _orchardServices;
        private readonly ILockFileManager _lockFileManager;
        private readonly IClock _clock;
        private readonly IAppDataFolder _appDataFolder;
        private readonly IWebDownloader _webDownloader;
        private const string BaseFolder = "Warmup";
        private const string WarmupFilename = "warmup.txt";
        private readonly string _lockFilename;

        public WarmupUpdater(
            IOrchardServices orchardServices, 
            ILockFileManager lockFileManager,
            IClock clock,
            IAppDataFolder appDataFolder,
            IWebDownloader webDownloader) {
            _orchardServices = orchardServices;
            _lockFileManager = lockFileManager;
            _clock = clock;
            _appDataFolder = appDataFolder;
            _webDownloader = webDownloader;
            _lockFilename = _appDataFolder.Combine(BaseFolder, WarmupFilename + ".lock");

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void EnsureGenerate() {
            var baseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var part = _orchardServices.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            // do nothing while the base url setting is not defined, or if there is no page defined
            if (String.IsNullOrWhiteSpace(baseUrl) || String.IsNullOrWhiteSpace(part.Urls)) {
                return;
            }

            // prevent multiple appdomains from rebuilding the static page concurrently (e.g., command line)
            ILockFile lockFile = null;
            if (!_lockFileManager.TryAcquireLock(_lockFilename, ref lockFile)) {
                return;
            }

            using (lockFile) {

                // check if we need to regenerate the pages by reading the last time it has been done
                // 1- if the warmup file doesn't exists, generate the pages
                // 2- otherwise, if the scheduled generation option is on, check if the delay is over
                var warmupPath = _appDataFolder.Combine(BaseFolder, WarmupFilename);
                if(_appDataFolder.FileExists(warmupPath)) {
                    try {
                        var warmupContent = _appDataFolder.ReadFile(warmupPath);
                        var expired = XmlConvert.ToDateTimeOffset(warmupContent).AddMinutes(part.Delay);
                        if (expired > _clock.UtcNow) {
                            return;
                        }
                    }
                    catch {
                        // invalid file, delete continue processing
                        _appDataFolder.DeleteFile(warmupPath);
                    }
                }

                // delete existing static page files
                foreach (var filename in _appDataFolder.ListFiles(BaseFolder)) {
                    var prefix = _appDataFolder.Combine(BaseFolder, "http");

                    // delete only static page files
                    if (!filename.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    try {
                        _appDataFolder.DeleteFile(filename);
                    }
                    catch(Exception e) {
                        // ignore files which could not be deleted
                        Logger.Error(e, "Could not delete file {0}", filename);
                    }
                }

                // loop over every relative url to generate the contents
                using (var urlReader = new StringReader(part.Urls)) {
                    string relativeUrl;
                    while (null != (relativeUrl = urlReader.ReadLine())) {
                        string url = null;
                        relativeUrl = relativeUrl.Trim();

                        try {
                            url = VirtualPathUtility.RemoveTrailingSlash(baseUrl) + relativeUrl;
                            var download = _webDownloader.Download(url);

                            if (download != null && download.StatusCode == HttpStatusCode.OK) {
                                var filename = WarmupUtility.EncodeUrl(url.TrimEnd('/'));
                                var path = _appDataFolder.Combine(BaseFolder, filename);
                                _appDataFolder.CreateFile(path, download.Content);

                                // if the base url contains http://www, then also render the www-less one

                                if (url.StartsWith("http://www.", StringComparison.OrdinalIgnoreCase)) {
                                    url = "http://" + url.Substring("http://www.".Length);
                                    filename = WarmupUtility.EncodeUrl(url.TrimEnd('/'));
                                    path = _appDataFolder.Combine(BaseFolder, filename);
                                    _appDataFolder.CreateFile(path, download.Content);
                                }

                            }
                        }
                        catch (Exception e) {
                            Logger.Error(e, "Could not extract warmup page content for: ", url);
                        }
                    }
                }

                // finally write the time the generation has been executed
                _appDataFolder.CreateFile(warmupPath, XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc));
            }
        }

        public void Generate() {
            // prevent multiple appdomains from rebuilding the static page concurrently (e.g., command line)
            ILockFile lockFile = null;
            if (!_lockFileManager.TryAcquireLock(_lockFilename, ref lockFile)) {
                return;
            }

            using (lockFile) {
                var warmupPath = _appDataFolder.Combine(BaseFolder, WarmupFilename);
                if (_appDataFolder.FileExists(warmupPath)) {
                    _appDataFolder.DeleteFile(warmupPath);
                }
            }

            EnsureGenerate();
        }
    }
}
