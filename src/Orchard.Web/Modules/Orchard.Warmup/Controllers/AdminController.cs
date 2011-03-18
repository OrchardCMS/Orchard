using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.Environment.Warmup;
using Orchard.FileSystems.AppData;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Warmup.Models;
using Orchard.UI.Notify;
using Orchard.Warmup.Services;

namespace Orchard.Warmup.Controllers {
    public  class AdminController : Controller, IUpdateModel {
        private readonly IWarmupScheduler _warmupScheduler;
        private readonly IAppDataFolder _appDataFolder;

        public AdminController(
            IOrchardServices services, 
            IWarmupScheduler warmupScheduler,
            IAppDataFolder appDataFolder) {
            _warmupScheduler = warmupScheduler;
            _appDataFolder = appDataFolder;
            Services = services;

            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var warmupPart = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();
            return View(warmupPart);
        }

        [FormValueRequired("submit")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPost() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to manage settings")))
                return new HttpUnauthorizedResult();

            var warmupPart = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            if(TryUpdateModel(warmupPart)) {
                Services.Notifier.Information(T("Warmup updated successfully."));
            }

            if (warmupPart.Scheduled) {
                if (warmupPart.Delay <= 0) {
                    AddModelError("Delay", T("Delay must be greater than zero."));
                }
            }

            return View(warmupPart);
        }

        [FormValueRequired("submit.Generate")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPostGenerate() {
            var result = IndexPost();
            
            if (ModelState.IsValid) {
                _warmupScheduler.Schedule(true);
                Services.Notifier.Information(T("Static pages are currently being generated."));
            }

            return result;
        }


        [FormValueRequired("submit.Extract")]
        [HttpPost, ActionName("Index")]
        public ActionResult IndexPostExtract() {
            var baseUrl = Services.WorkContext.CurrentSite.BaseUrl;
            baseUrl = VirtualPathUtility.AppendTrailingSlash(baseUrl);

            var part = Services.WorkContext.CurrentSite.As<WarmupSettingsPart>();

            if (String.IsNullOrWhiteSpace(baseUrl) || String.IsNullOrWhiteSpace(part.Urls)) {
                return RedirectToAction("Index");
            }

            var regex = new Regex(@"<link\s[^>]*href=""(?<url>[^""]*\.css)""|<script\s[^>]*src=""(?<url>[^""]*\.js)""", RegexOptions.IgnoreCase);
            var resources = new List<string>();

            // add the already registered urls to remove duplicates
            using (var urlReader = new StringReader(part.Urls)) {
                string relativeUrl;
                while (null != (relativeUrl = urlReader.ReadLine())) {
                    if (String.IsNullOrWhiteSpace(relativeUrl)) {
                        continue;
                    }

                    relativeUrl = relativeUrl.Trim();
                    resources.Add(relativeUrl);

                    try {
                        var contentUrl = VirtualPathUtility.RemoveTrailingSlash(baseUrl) + relativeUrl;
                        var filename = WarmupUtility.EncodeUrl(contentUrl.TrimEnd('/'));
                        var path = _appDataFolder.Combine("Warmup", filename);

                        if(!_appDataFolder.FileExists(path)) {
                            continue;
                        }

                        var content = _appDataFolder.ReadFile(path);

                        // process only html files
                        if (!content.Contains("<html") && !content.Contains("</html")) {
                            continue;
                        }

                        var localPrefix = Request.ApplicationPath ?? "/";

                        var matches = regex.Matches(content);
                        foreach (Match m in matches) {
                            var url = m.Groups["url"].Value;

                            if (url.StartsWith(localPrefix, StringComparison.OrdinalIgnoreCase)) {
                                resources.Add(url.Substring(localPrefix.Length));
                            }
                            else if (url.StartsWith(baseUrl, StringComparison.OrdinalIgnoreCase)) {
                                resources.Add("/" + url.Substring(baseUrl.Length));
                            }
                            else if (!url.StartsWith("http://") && !url.StartsWith("/")) {
                                // relative urls e.g., ../, foo.js, ...
                                relativeUrl = VirtualPathUtility.AppendTrailingSlash(relativeUrl);
                                url = VirtualPathUtility.Combine(relativeUrl, url);
                                resources.Add(url);
                            }
                        }
                    }
                    catch {
                        // if something unexpected happens, process next file
                        continue;
                    }
                }
            }

            // extract unique urls
            var uniqueResources = resources.GroupBy(x => x.ToLowerInvariant()).Select(x => x.First()).ToArray();
            part.Urls = String.Join(System.Environment.NewLine, uniqueResources);
            
            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}