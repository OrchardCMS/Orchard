using System;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.UI.Notify;

namespace Orchard.Search.Controllers {
    public class AdminController : Controller {
        private readonly ISearchService _searchService;

        public AdminController(ISearchService searchService, IOrchardServices services) {
            _searchService = searchService;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        public ActionResult Index() {
            var viewModel = new SearchIndexViewModel {HasIndexToManage = _searchService.HasIndexToManage, IndexUpdatedUtc = _searchService.GetIndexUpdatedUtc()};

            if (!viewModel.HasIndexToManage)
                Services.Notifier.Information(T("There is not search index to manage for this site."));

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Update() {
            if (!Services.Authorizer.Authorize(Permissions.ManageSearchIndex, T("Not allowed to manage the search index.")))
                return new HttpUnauthorizedResult();

            _searchService.UpdateIndex();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Rebuild() {
            if (!Services.Authorizer.Authorize(Permissions.ManageSearchIndex, T("Not allowed to manage the search index.")))
                return new HttpUnauthorizedResult();

            _searchService.RebuildIndex();

            return RedirectToAction("Index");
        }
    }
}