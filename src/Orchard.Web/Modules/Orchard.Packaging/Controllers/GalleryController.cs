using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Packaging.Services;
using Orchard.Packaging.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using System.Xml.XPath;

namespace Orchard.Packaging.Controllers {
    [OrchardFeature("Gallery")]
    [Themed, Admin]
    public class GalleryController : Controller {

        private readonly IPackageManager _packageManager;
        private readonly IPackagingSourceManager _packagingSourceManager;
        private readonly IExtensionManager _extensionManager;
        private readonly INotifier _notifier;

        public GalleryController(
            IPackageManager packageManager,
            IPackagingSourceManager packagingSourceManager,
            IExtensionManager extensionManager,
            INotifier notifier) {
            _packageManager = packageManager;
            _packagingSourceManager = packagingSourceManager;
            _extensionManager = extensionManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        public ActionResult Sources() {
            return View(new PackagingSourcesViewModel {
                Sources = _packagingSourceManager.GetSources(),
            });
        }

        public ActionResult Remove(int id) {
            _packagingSourceManager.RemoveSource(id);
            _notifier.Information(T("The feed has been removed successfully."));
            return RedirectToAction("Sources");
        }

        public ActionResult AddSource() {
            return View(new PackagingAddSourceViewModel());
        }

        [HttpPost]
        public ActionResult AddSource(string url) {
            try {
                if ( !String.IsNullOrEmpty(url) ) {
                    if (!url.StartsWith("http")) {
                        ModelState.AddModelError("Url", T("The Url is not valid").Text);
                    }
                }
                else if ( String.IsNullOrWhiteSpace(url)) {
                    ModelState.AddModelError("Url", T("Url is required").Text);
                }

                string title = null;
                // try to load the feed
                try {

                    XNamespace atomns = "http://www.w3.org/2005/Atom" ;
                    var feed = XDocument.Load(url, LoadOptions.PreserveWhitespace);
                    var titleNode = feed.Descendants(atomns + "title").FirstOrDefault();
                    if ( titleNode != null )
                        title = titleNode.Value;

                    if(String.IsNullOrWhiteSpace(title)) {
                        ModelState.AddModelError("Url", T("The feed has no title.").Text);
                    }
                }
                catch {
                    ModelState.AddModelError("Url", T("The url of the feed or its content is not valid.").Text);
                }

                if ( !ModelState.IsValid )
                    return View(new PackagingAddSourceViewModel { Url = url });

                _packagingSourceManager.AddSource(title, url);
                _notifier.Information(T("The feed has been added successfully."));

                return RedirectToAction("Sources");
            }
            catch ( Exception exception ) {
                _notifier.Error(T("Adding feed failed: {0}", exception.Message));
                return View(new PackagingAddSourceViewModel { Url = url });
            }
        }


        public ActionResult Modules(int? sourceId) {
            var selectedSource = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            var sources = selectedSource != null 
                ? new [] { selectedSource }
                : _packagingSourceManager.GetSources()
            ;

            return View("Modules", new PackagingExtensionsViewModel {
                Extensions = sources.SelectMany(source => _packagingSourceManager.GetModuleList(source)),
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                SelectedSource = selectedSource
            });
        }

        public ActionResult Themes(int? sourceId) {
            var selectedSource = _packagingSourceManager.GetSources().Where(s => s.Id == sourceId).FirstOrDefault();

            var sources = selectedSource != null
                ? new[] { selectedSource }
                : _packagingSourceManager.GetSources()
            ;

            return View("Themes", new PackagingExtensionsViewModel {
                Extensions = sources.SelectMany(source => _packagingSourceManager.GetThemeList(source)),
                Sources = _packagingSourceManager.GetSources().OrderBy(s => s.FeedTitle),
                SelectedSource = selectedSource
            });
        }

        public ActionResult Harvest(string extensionName, string feedUrl) {
            return View(new PackagingHarvestViewModel {
                ExtensionName = extensionName,
                FeedUrl = feedUrl,
                Sources = _packagingSourceManager.GetSources(),
                Extensions = _extensionManager.AvailableExtensions()
            });
        }

        [HttpPost]
        public ActionResult Harvest(PackagingHarvestViewModel model) {
            #if REFACTORING
            model.Sources = _packagingSourceManager.GetSources();
            model.Extensions = _extensionManager.AvailableExtensions();

            var packageData = _packageManager.Harvest(model.ExtensionName);

            if (string.IsNullOrEmpty(model.FeedUrl)) {
                return new DownloadStreamResult(
                    packageData.ExtensionName + "-" + packageData.ExtensionVersion + ".zip",
                    "application/x-package",
                    packageData.PackageStream);
            }

            if (!model.Sources.Any(src => src.FeedUrl == model.FeedUrl)) {
                ModelState.AddModelError("FeedUrl", T("May only push directly to one of the configured sources.").ToString());
                return View(model);
            }

            _packageManager.Push(packageData, model.FeedUrl, model.User, model.Password);
            _notifier.Information(T("Harvested {0} and published onto {1}", model.ExtensionName, model.FeedUrl));

            Update(null);

            return RedirectToAction("Harvest", new { model.ExtensionName, model.FeedUrl });
#else
            return View();
#endif

        }

        public ActionResult Install(string syndicationId, string cameFrom) {
#if REFACTORING
            var packageData = _packageManager.Download(syndicationId);
            _packageManager.Install(packageData.PackageStream);
            _notifier.Information(T("Installed module"));
#endif
            return RedirectToAction(cameFrom == "Themes" ? "ThemesIndex" : "ModulesIndex");
        }
    }
}