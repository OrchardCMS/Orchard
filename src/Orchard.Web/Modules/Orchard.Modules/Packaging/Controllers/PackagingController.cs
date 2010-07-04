using System;
using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Orchard.Modules.Packaging.Services;
using Orchard.Modules.Packaging.ViewModels;
using Orchard.Themes;
using Orchard.UI.Admin;

namespace Orchard.Modules.Packaging.Controllers {
    [Admin, Themed, OrchardFeature("Orchard.Modules.Packaging")]
    public class PackagingController : Controller {
        private readonly IPackageRepository _packageRepository;

        public PackagingController(IPackageRepository packageRepository) {
            _packageRepository = packageRepository;
        }

        public ActionResult Index() {
            return Modules();
        }

        public ActionResult Modules() {
            return View("Modules", new PackagingIndexViewModel {
                Modules = _packageRepository.GetModuleList()
            });
        }

        public ActionResult Sources() {
            return View("Sources", new PackagingIndexViewModel {
                Sources = _packageRepository.GetSources(),
            });
        }

        public ActionResult AddSource(string url) {
            _packageRepository.AddSource(new PackageSource { Id = Guid.NewGuid(), FeedUrl = url });
            return RedirectToAction("Index");
        }

        public ActionResult Update() {
            _packageRepository.UpdateLists();
            //notifier
            return RedirectToAction("Index");
        }

    }
}