using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Modules.Packaging.Services;
using Orchard.Modules.Packaging.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace Orchard.Modules.Packaging.Controllers {
    [Admin, Themed, OrchardFeature("Orchard.Modules.Packaging")]
    public class PackagingController : Controller {
        private readonly IPackageManager _packageManager;
        private readonly IPackageSourceManager _packageSourceManager;
        private readonly IExtensionManager _extensionManager;
        private readonly INotifier _notifier;

        public PackagingController(
            IPackageManager packageManager,
            IPackageSourceManager packageSourceManager,
            IExtensionManager extensionManager,
            INotifier notifier) {
            _packageManager = packageManager;
            _packageSourceManager = packageSourceManager;
            _extensionManager = extensionManager;
            _notifier = notifier;
            T = NullLocalizer.Instance;
        }

        Localizer T { get; set; }

        public ActionResult Index() {
            return Modules();
        }

        public ActionResult Sources() {
            return View("Sources", new PackagingSourcesViewModel {
                Sources = _packageSourceManager.GetSources(),
            });
        }

        public ActionResult AddSource(string url) {
            _packageSourceManager.AddSource(new PackageSource { Id = Guid.NewGuid(), FeedUrl = url });
            Update();
            return RedirectToAction("Sources");
        }


        public ActionResult Modules() {
            return View("Modules", new PackagingModulesViewModel {
                Modules = _packageSourceManager.GetModuleList()
            });
        }

        public ActionResult Update() {
            _packageSourceManager.UpdateLists();
            _notifier.Information(T("List of available modules and themes is updated."));
            return RedirectToAction("Index");
        }

        public ActionResult Harvest(string extensionName, string feedUrl) {
            return View("Harvest", new PackagingHarvestViewModel {
                ExtensionName = extensionName,
                FeedUrl = feedUrl,
                Sources = _packageSourceManager.GetSources(),
                Extensions = _extensionManager.AvailableExtensions()
            });
        }

        [HttpPost]
        public ActionResult Harvest(PackagingHarvestViewModel model) {
            model.Sources = _packageSourceManager.GetSources();
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
                return View("Harvest", model);
            }

            _packageManager.Push(packageData, model.FeedUrl);
            _notifier.Information(T("Harvested {0} and published onto {1}", model.ExtensionName, model.FeedUrl));

            Update();

            return RedirectToAction("Harvest", new { model.ExtensionName, model.FeedUrl });
        }

        public ActionResult Install(string syndicationId) {
            var packageData = _packageManager.Download(syndicationId);
            _packageManager.Install(packageData);
            _notifier.Information(T("Installed module"));
            return RedirectToAction("Modules");
        }
    }
}