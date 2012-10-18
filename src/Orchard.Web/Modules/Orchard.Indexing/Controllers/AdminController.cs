using System;
using System.Web.Mvc;
using Orchard.Indexing.Services;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Indexing.ViewModels;

namespace Orchard.Indexing.Controllers {
    public class AdminController : Controller {
        private readonly IIndexingService _indexingService;
        private const string DefaultIndexName = "Search";

        public AdminController(IIndexingService indexingService, IOrchardServices services) {
            _indexingService = indexingService;
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index() {
            var viewModel = new IndexViewModel();
            
            try {
                viewModel.IndexEntry = _indexingService.GetIndexEntry(DefaultIndexName);

                if (viewModel.IndexEntry == null)
                    Services.Notifier.Information(T("There is no search index to manage for this site."));
            }
            catch(Exception e) {
                Logger.Error(e, "Search index couldn't be read.");
                Services.Notifier.Information(T("The index might be corrupted. If you can't recover click on Rebuild."));
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Update() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage the search index.")))
                return new HttpUnauthorizedResult();

            _indexingService.UpdateIndex(DefaultIndexName);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Rebuild() {
            if (!Services.Authorizer.Authorize(StandardPermissions.SiteOwner, T("Not allowed to manage the search index.")))
                return new HttpUnauthorizedResult();

            _indexingService.RebuildIndex(DefaultIndexName);

            return RedirectToAction("Index");
        }
    }
}