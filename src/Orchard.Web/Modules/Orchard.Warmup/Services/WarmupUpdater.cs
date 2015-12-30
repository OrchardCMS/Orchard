using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
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
        private readonly IWarmupReportManager _reportManager;
        private const string BaseFolder = "Warmup";
        private const string WarmupFilename = "warmup.txt";
        
        private readonly string _warmupPath;
        private readonly string _lockFilename;

        public WarmupUpdater(
            IOrchardServices orchardServices, 
            ILockFileManager lockFileManager,
            IClock clock,
            IAppDataFolder appDataFolder,
            IWebDownloader webDownloader,
            IWarmupReportManager reportManager,
            ShellSettings shellSettings) {
            _orchardServices = orchardServices;
            _lockFileManager = lockFileManager;
            _clock = clock;
            _appDataFolder = appDataFolder;
            _webDownloader = webDownloader;
            _reportManager = reportManager;

            _lockFilename = _appDataFolder.Combine("Sites", _appDataFolder.Combine(shellSettings.Name, WarmupFilename + ".lock"));
            _warmupPath = _appDataFolder.Combine("Sites", _appDataFolder.Combine(shellSettings.Name, WarmupFilename));

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void EnsureGenerate() {
            var baseUrl = _orchardServices.WorkContext.CurrentSite.BaseUrl;
            var part = _orchardServices.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            // do nothing while the base url setting is not defined
            if (String.IsNullOrWhiteSpace(baseUrl)) {
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
                if (_appDataFolder.FileExists(_warmupPath)) {
                    try {
                        var warmupContent = _appDataFolder.ReadFile(_warmupPath);
                        var expired = XmlConvert.ToDateTimeOffset(warmupContent).AddMinutes(part.Delay);
                        if (expired > _clock.UtcNow) {
                            return;
                        }
                    }
                    catch {
                        // invalid file, delete continue processing
                        _appDataFolder.DeleteFile(_warmupPath);
                    }
                }

                // delete peviously generated pages, by reading the Warmup Report file
                try {
                    var encodedPrefix = WarmupUtility.EncodeUrl("http://www.");

                    foreach (var reportEntry in _reportManager.Read()) {
                        try {
                            // use FileName as the SiteBaseUrl could have changed in the meantime
                            var path = _appDataFolder.Combine(BaseFolder, reportEntry.Filename);
                            _appDataFolder.DeleteFile(path);

                            // delete the www-less version too if it's available
                            if (reportEntry.Filename.StartsWith(encodedPrefix, StringComparison.OrdinalIgnoreCase)) {
                                var filename = WarmupUtility.EncodeUrl("http://") + reportEntry.Filename.Substring(encodedPrefix.Length);
                                path = _appDataFolder.Combine(BaseFolder, filename);
                                _appDataFolder.DeleteFile(path);
                            }
                        }
                        catch (Exception e) {
                            Logger.Error(e, "Could not delete specific warmup file: ", reportEntry.Filename);
                        }
                    }
                }
                catch(Exception e) {
                    Logger.Error(e, "Could not read warmup report file");
                }

                var reportEntries = new List<ReportEntry>();

                if (!String.IsNullOrEmpty(part.Urls)) {
                    // loop over every relative url to generate the contents
                    using (var urlReader = new StringReader(part.Urls)) {
                        string relativeUrl;
                        while (null != (relativeUrl = urlReader.ReadLine())) {
                            if (String.IsNullOrWhiteSpace(relativeUrl)) {
                                continue;
                            }

                            string url = null;
                            relativeUrl = relativeUrl.Trim();

                            try {
                                url = VirtualPathUtility.RemoveTrailingSlash(baseUrl) + relativeUrl;
                                var filename = WarmupUtility.EncodeUrl(url.TrimEnd('/'));
                                var path = _appDataFolder.Combine(BaseFolder, filename);

                                var download = _webDownloader.Download(url);

                                if (download != null) {
                                    if (download.StatusCode == HttpStatusCode.OK) {
                                        // success
                                        _appDataFolder.CreateFile(path, download.Content);

                                        reportEntries.Add(new ReportEntry {
                                            RelativeUrl = relativeUrl,
                                            Filename = filename,
                                            StatusCode = (int) download.StatusCode,
                                            CreatedUtc = _clock.UtcNow
                                        });

                                        // if the base url contains http://www, then also render the www-less one);

                                        if (url.StartsWith("http://www.", StringComparison.OrdinalIgnoreCase)) {
                                            url = "http://" + url.Substring("http://www.".Length);
                                            filename = WarmupUtility.EncodeUrl(url.TrimEnd('/'));
                                            path = _appDataFolder.Combine(BaseFolder, filename);
                                            _appDataFolder.CreateFile(path, download.Content);
                                        }
                                    }
                                    else {
                                        reportEntries.Add(new ReportEntry {
                                            RelativeUrl = relativeUrl,
                                            Filename = filename,
                                            StatusCode = (int) download.StatusCode,
                                            CreatedUtc = _clock.UtcNow
                                        });
                                    }
                                }
                                else {
                                    // download failed
                                    reportEntries.Add(new ReportEntry {
                                        RelativeUrl = relativeUrl,
                                        Filename = filename,
                                        StatusCode = 0,
                                        CreatedUtc = _clock.UtcNow
                                    });
                                }
                            }
                            catch (Exception e) {
                                Logger.Error(e, "Could not extract warmup page content for: ", url);
                            }
                        }
                    }
                }

                _reportManager.Create(reportEntries);

                // finally write the time the generation has been executed
                _appDataFolder.CreateFile(_warmupPath, XmlConvert.ToString(_clock.UtcNow, XmlDateTimeSerializationMode.Utc));
            }
        }

        public void Generate() {
            // prevent multiple appdomains from rebuilding the static page concurrently (e.g., command line)
            ILockFile lockFile = null;
            if (!_lockFileManager.TryAcquireLock(_lockFilename, ref lockFile)) {
                return;
            }

            using (lockFile) {
                if (_appDataFolder.FileExists(_warmupPath)) {
                    _appDataFolder.DeleteFile(_warmupPath);
                }
            }

            EnsureGenerate();
        }
    }
}
