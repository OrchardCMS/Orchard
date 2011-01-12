using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Packaging.Models;
using Orchard.Packaging.Services;
using Orchard.Packaging.ViewModels;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using IPackageManager = Orchard.Packaging.Services.IPackageManager;

namespace Orchard.Packaging.Controllers {
    [OrchardFeature("Gallery")]
    [Themed, Admin]
    public class GalleryController : Controller {

        private readonly IPackageManager _packageManager;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly INotifier _notifier;

        public GalleryController(
            IPackageManager packageManager,
            IPackagingSourceManager packagingSourceManager,
            INotifier notifier,
            IOrchardServices services) {
            _packageManager = packageManager;
            _packagingSourceManager = packagingSourceManager;
            _notifier = notifier;
            Services = services;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Sources() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list sources")))
                return new HttpUnauthorizedResult();

            return View(new PackagingSourcesViewModel {
                Sources = _packagingSourceManager.GetSources(),
            });
        }

        public ActionResult Remove(int id) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to remove sources")))
                return new HttpUnauthorizedResult();

            _packagingSourceManager.RemoveSource(id);
            _notifier.Information(T("The feed has been removed successfully."));
            return RedirectToAction("Sources");
        }

        public ActionResult AddSource() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            return View(new PackagingAddSourceViewModel());
        }

        [HttpPost]
        public ActionResult AddSource(string url) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to add sources")))
                return new HttpUnauthorizedResult();

            try {
                if (!String.IsNullOrEmpty(url)) {
                    if (!url.StartsWith("http")) {
                        ModelState.AddModelError("Url", T("The Url is not valid").Text);
                    }
                }
                else if (String.IsNullOrWhiteSpace(url)) {
                    ModelState.AddModelError("Url", T("Url is required").Text);
                }

                string title = null;
                // try to load the feed
                try {

                    XNamespace atomns = "http://www.w3.org/2005/Atom";
                    var feed = XDocument.Load(url, LoadOptions.PreserveWhitespace);
                    var titleNode = feed.Descendants(atomns + "title").FirstOrDefault();
                    if (titleNode != null)
                        title = titleNode.Value;

                    if (String.IsNullOrWhiteSpace(title)) {
                        ModelState.AddModelError("Url", T("The feed has no title.").Text);
                    }
                }
                catch {
                    ModelState.AddModelError("Url", T("The url of the feed or its content is not valid.").Text);
                }

                if (!ModelState.IsValid)
                    return View(new PackagingAddSourceViewModel { Url = url });

                _packagingSourceManager.AddSource(title, url);
                _notifier.Information(T("The feed has been added successfully."));

                return RedirectToAction("Sources");
            }
            catch (Exception exception) {
                _notifier.Error(T("Adding feed failed: {0}", exception.Message));
                return View(new PackagingAddSourceViewModel { Url = url });
            }
        }

        public ActionResult Modules(int? sourceId) {
            return ListExtensions(sourceId, DefaultExtensionTypes.Module, "Modules", source => _packagingSourceManager.GetModuleList(source).ToArray());
        }

        public ActionResult Themes(int? sourceId) {
            return ListExtensions(sourceId, DefaultExtensionTypes.Theme, "Themes", source => _packagingSourceManager.GetThemeList(source).ToArray());
        }

        protected ActionResult ListExtensions(int? sourceId, string extensionType, string returnView, Func<PackagingSource, PackagingEntry[]> getList) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to list {0}", extensionType)))
                return new HttpUnauthorizedResult();

            var selectedSource = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            var sources = selectedSource != null
                ? new[] { selectedSource }
                : _packagingSourceManager.GetSources()
            ;

            IEnumerable<PackagingEntry> extensions = null;
            foreach (var source in sources) {
                try {
                    var sourceExtensions = getList(source);
                    extensions = extensions == null ? sourceExtensions : extensions.Concat(sourceExtensions);
                }
                catch (Exception ex) {
                    Logger.Error(ex, "Error loading extensions from gallery source '{0}'. {1}.", source.FeedTitle, ex.Message);
                    _notifier.Error(T("Error loading extensions from gallery source '{0}'. {1}.", source.FeedTitle, ex.Message));
                }
            }

            return View(returnView, new PackagingExtensionsViewModel {
                Extensions = extensions ?? new PackagingEntry[] { },
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                SelectedSource = selectedSource
            });
        }

        public ActionResult Install(string packageId, string version, int sourceId, string redirectTo) {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not authorized to install packages")))
                return new HttpUnauthorizedResult();

            var source = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            if (source == null) {
                return HttpNotFound();
            }

            try {
                _packageManager.Install(packageId, version, source.FeedUrl, HostingEnvironment.MapPath("~/"));
            }
            catch (Exception exception) {
                _notifier.Error(T("Package installation failed."));
                for (Exception scan = exception; scan != null; scan = scan.InnerException) {
                    _notifier.Error(T("{0}", scan.Message));
                }
            }

            return RedirectToAction(redirectTo == "Themes" ? "Themes" : "Modules");
        }
    }
}