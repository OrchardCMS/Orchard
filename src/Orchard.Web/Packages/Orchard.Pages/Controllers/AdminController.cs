using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Mvc.Results;
using Orchard.Pages.Models;
using Orchard.Pages.Services;
using Orchard.Pages.ViewModels;
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

        public ActionResult List(PagesOptions options) {
            // Default options
            if (options == null)
                options = new PagesOptions();

            IEnumerable<Page> pages;
            // Filtering
            switch (options.Filter) {
                case PagesFilter.All:
                    pages = _pageService.Get();
                    break;
                case PagesFilter.Published:
                    pages = _pageService.Get(PageStatus.Published);
                    break;
                case PagesFilter.Offline:
                    pages = _pageService.Get(PageStatus.Offline);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var entries = pages.Select(page => CreatePageEntry(page)).ToList();
            var model = new PagesViewModel { Options = options, PageEntries = entries };
            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(PagesOptions options, IList<PageEntry> pageEntries) {
            IEnumerable<PageEntry> checkedEntries = pageEntries.Where(p => p.IsChecked);
            switch (options.BulkAction) {
                case PagesBulkAction.None:
                    break;
                case PagesBulkAction.PublishNow:
                    if (!Services.Authorizer.Authorize(Permissions.PublishPages, T("Couldn't publish page")))
                        return new HttpUnauthorizedResult();

                    foreach (PageEntry entry in checkedEntries) {
                        var page = _pageService.GetLatest(entry.PageId);
                        _pageService.Publish(page);
                    }
                    break;
                case PagesBulkAction.Unpublish:
                    if (!Services.Authorizer.Authorize(Permissions.UnpublishPages, T("Couldn't unpublish page")))
                        return new HttpUnauthorizedResult();
                    foreach (PageEntry entry in checkedEntries) {
                        var page = _pageService.GetLatest(entry.PageId);
                        _pageService.Unpublish(page);
                    }
                    break;
                case PagesBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.DeletePages, T("Couldn't delete page")))
                        return new HttpUnauthorizedResult();

                    foreach (PageEntry entry in checkedEntries) {
                        var page = _pageService.GetLatest(entry.PageId);
                        _pageService.Delete(page);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return RedirectToAction("List");
        }

        private static PageEntry CreatePageEntry(Page page) {
            return new PageEntry {
                Page = page,
                IsChecked = false,
                PageId = page.Id
            };
        }

        public ActionResult Create() {
            if (!Services.Authorizer.Authorize(Permissions.CreatePages, T("Not allowed to create a page")))
                return new HttpUnauthorizedResult();

            var page = Services.ContentManager.BuildEditorModel(_pageService.New());

            var model = new PageCreateViewModel {
                Page = page
            };

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(PageCreateViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.CreatePages, T("Couldn't create page")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            } else if (string.Equals(Request.Form["Publish"], "Publish")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Publish"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            Page page = _pageService.Create(publishNow, publishDate);
            model.Page = Services.ContentManager.UpdateEditorModel(page, this);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            var session = _sessionLocator.For(typeof(Page));
            session.Flush();

            return RedirectToAction("List");
        }

        public ActionResult Edit(string pageSlug) {
            if (!Services.Authorizer.Authorize(Permissions.ModifyPages, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();

            Page page = _pageService.GetLatest(pageSlug);

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

            Page page = _pageService.GetPageOrDraft(pageSlug);

            if (page == null)
                return new NotFoundResult();

            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            } else if (string.Equals(Request.Form["Publish"], "Publish")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Publish"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            //TODO: (erikpo) Move this duplicate code somewhere else
            if (publishNow)
                _pageService.Publish(page);
            else if (publishDate != null)
                _pageService.Publish(page, publishDate.Value);
            else
                _pageService.Unpublish(page);

            var model = new PageEditViewModel {
                Page = Services.ContentManager.UpdateEditorModel(page, this)
            };

            TryUpdateModel(model);

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();

                return View(model);
            }

            Services.Notifier.Information(T("Page information updated."));

            return RedirectToAction("List");
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

        public class FormValueRequiredAttribute : ActionMethodSelectorAttribute {
            private readonly string _submitButtonName;

            public FormValueRequiredAttribute(string submitButtonName) {
                _submitButtonName = submitButtonName;
            }

            public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) {
                var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
                return !string.IsNullOrEmpty(value);
            }
        }
    }
}