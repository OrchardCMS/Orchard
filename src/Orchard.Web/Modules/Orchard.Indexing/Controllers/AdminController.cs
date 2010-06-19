using System.Web.Mvc;
using Orchard.Indexing.Services;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Indexing.ViewModels;

namespace Orchard.Indexing.Controllers {
    public class AdminController : Controller {
        private readonly IIndexingService _indexingService;

        public AdminController(IIndexingService indexingService, IOrchardServices services) {
            _indexingService = indexingService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var viewModel = new IndexViewModel {HasIndexToManage = _indexingService.HasIndexToManage, IndexUpdatedUtc = _indexingService.GetIndexUpdatedUtc()};

            if (!viewModel.HasIndexToManage)
                Services.Notifier.Information(T("There is no search index to manage for this site."));

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Update() {
            if ( !Services.Authorizer.Authorize(Permissions.ManageSearchIndex, T("Not allowed to manage the search index.")) )
                return new HttpUnauthorizedResult();

            _indexingService.UpdateIndex();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Rebuild() {
            if ( !Services.Authorizer.Authorize(Permissions.ManageSearchIndex, T("Not allowed to manage the search index.")) )
                return new HttpUnauthorizedResult();

            _indexingService.RebuildIndex();
            _indexingService.UpdateIndex();

            return RedirectToAction("Index");
        }
    }
}