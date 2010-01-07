using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Mvc.Results;
using Orchard.Pages.Models;
using Orchard.Pages.Services;
using Orchard.Pages.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Pages.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IPageService _pageService;
        private readonly ISlugConstraint _slugConstraint;

        public AdminController(
            IOrchardServices services,
            ISessionLocator sessionLocator,
            IPageService pageService,
            ISlugConstraint slugConstraint) {
            Services = services;
            _sessionLocator = sessionLocator;
            _pageService = pageService;
            _slugConstraint = slugConstraint;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        private Localizer T { get; set; }

        public ActionResult List() {
            IEnumerable<Page> pages = _pageService.Get();
            var model = new PagesViewModel {
                Pages = pages
            };

            return View(model);
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.CreatePages, T("Not allowed to create a page")))
                return new HttpUnauthorizedResult();

            var page = Services.ContentManager.BuildEditorModel(Services.ContentManager.New<Page>("page"));

            var model = new PageCreateViewModel {
                Page = page
            };

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(PageCreateViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.CreatePages, T("Couldn't create page")))
                return new HttpUnauthorizedResult();

            Page page = Services.ContentManager.Create<Page>("page");
            model.Page = Services.ContentManager.UpdateEditorModel(page, this);

            if (!ModelState.IsValid)
                return View(model);

            var session = _sessionLocator.For(typeof(Page));
            session.Flush();

            return RedirectToAction("Edit", new { pageSlug = model.Page.Item.Slug });
        }

        public ActionResult Edit(string pageSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ModifyPages, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.Get(pageSlug);

            if (page == null)
                return new NotFoundResult();

            var model = new PageEditViewModel {
                Page = Services.ContentManager.BuildEditorModel(page)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string pageSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ModifyPages, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.Get(pageSlug);

            if (page == null)
                return new NotFoundResult();

            var model = new PageEditViewModel {
                Page = Services.ContentManager.UpdateEditorModel(page, this)
            };

            TryUpdateModel(model);

            if (ModelState.IsValid == false) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.Notifier.Information(T("Page information updated."));
            return RedirectToAction("Edit", new { pageSlug = page.Slug });
        }

        [HttpPost]
        public ActionResult Delete(string pageSlug) {
            if (!Services.Authorizer.Authorize(Permissions.DeletePages, T("Couldn't delete page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.Get(pageSlug);

            if (page == null)
                return new NotFoundResult();

            _pageService.Delete(page);

            Services.Notifier.Information(T("Page was successfully deleted"));

            return RedirectToAction("List");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}