using System;
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
    public class PageController : Controller, IUpdateModel {
        private readonly ISessionLocator _sessionLocator;
        private readonly IContentManager _contentManager;
        private readonly IAuthorizer _authorizer;
        private readonly INotifier _notifier;
        private readonly IPageService _pageService;
        private readonly ISlugConstraint _slugConstraint;

        public PageController(
            IOrchardServices services,
            ISessionLocator sessionLocator, IContentManager contentManager,
            IAuthorizer authorizer,
            INotifier notifier,
            IPageService pageService,
            ISlugConstraint slugConstraint) {
            Services = services;
            _sessionLocator = sessionLocator;
            _contentManager = contentManager;
            _authorizer = authorizer;
            _notifier = notifier;
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

        public ActionResult Item(string slug) {
            if (!_authorizer.Authorize(Permissions.ViewPages, T("Couldn't view page")))
                return new HttpUnauthorizedResult();

            if (slug == null) {
                throw new ArgumentNullException("slug");
            }

            //var correctedSlug = _slugConstraint.LookupPublishedSlug(pageSlug);

            var page = _pageService.Get(slug);

            var model = new PageViewModel {
                Page = _contentManager.BuildDisplayModel(page, "Detail")
            };
            return View(model);
        }

        public ActionResult Create() {
            if (!_authorizer.Authorize(Permissions.CreatePages, T("Not allowed to create a page")))
                return new HttpUnauthorizedResult();

            var page = _contentManager.BuildEditorModel(_contentManager.New<Page>("page"));

            var model = new PageCreateViewModel {
                Page = page
            };

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(PageCreateViewModel model) {
            if (!_authorizer.Authorize(Permissions.CreatePages, T("Couldn't create page")))
                return new HttpUnauthorizedResult();

            Page page = _contentManager.Create<Page>("page");
            model.Page = _contentManager.UpdateEditorModel(page, this);

            if (!ModelState.IsValid)
                return View(model);

            var session = _sessionLocator.For(typeof(Page));
            session.Flush();

            return RedirectToAction("Edit", new { model.Page.Item.Slug });
        }

        public ActionResult Edit(string pageSlug) {
            if (!_authorizer.Authorize(Permissions.ModifyPages, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.Get(pageSlug);

            if (page == null)
                return new NotFoundResult();

            var model = new PageEditViewModel {
                Page = _contentManager.BuildEditorModel(page)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(string pageSlug) {
            if (!_authorizer.Authorize(Permissions.ModifyPages, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.Get(pageSlug);

            if (page == null)
                return new NotFoundResult();

            var model = new PageEditViewModel {
                Page = _contentManager.UpdateEditorModel(page, this)
            };

            TryUpdateModel(model);

            if (ModelState.IsValid == false) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            _notifier.Information(T("Page information updated."));
            return RedirectToAction("Edit", new { page.Slug });
        }

        [HttpPost]
        public ActionResult Delete(string pageSlug) {
            if (!_authorizer.Authorize(Permissions.DeletePages, T("Couldn't delete page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.Get(pageSlug);

            if (page == null)
                return new NotFoundResult();

            _pageService.Delete(page);

            _notifier.Information(T("Page was successfully deleted"));

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