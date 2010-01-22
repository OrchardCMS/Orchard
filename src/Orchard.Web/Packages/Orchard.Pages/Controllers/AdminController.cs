using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
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
        private readonly IPageService _pageService;

        public AdminController(IOrchardServices services, IPageService pageService) {
            Services = services;
            _pageService = pageService;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; private set; }
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
                        Services.ContentManager.Flush();
                    }
                    break;
                case PagesBulkAction.Unpublish:
                    if (!Services.Authorizer.Authorize(Permissions.PublishPages, T("Couldn't unpublish page")))
                        return new HttpUnauthorizedResult();
                    foreach (PageEntry entry in checkedEntries) {
                        var page = _pageService.GetLatest(entry.PageId);
                        _pageService.Unpublish(page);
                        Services.ContentManager.Flush();
                    }
                    break;
                case PagesBulkAction.Delete:
                    if (!Services.Authorizer.Authorize(Permissions.DeletePages, T("Couldn't delete page")))
                        return new HttpUnauthorizedResult();

                    foreach (PageEntry entry in checkedEntries) {
                        var page = _pageService.GetLatest(entry.PageId);
                        _pageService.Delete(page);
                        Services.ContentManager.Flush();
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
            if (!Services.Authorizer.Authorize(Permissions.EditPages, T("Not allowed to create a page")))
                return new HttpUnauthorizedResult();

            var page = Services.ContentManager.BuildEditorModel(Services.ContentManager.New<Page>("page"));

            var model = new PageCreateViewModel {
                Page = page
            };

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(PageCreateViewModel model) {
            if (!Services.Authorizer.Authorize(Permissions.EditPages, T("Couldn't create page")))
                return new HttpUnauthorizedResult();

            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            }
            else if (string.Equals(Request.Form["Command"], "PublishLater")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Published"], out publishDateValue)) {
                    publishDate = publishDateValue;
                }
            }

            model.Page = Services.ContentManager.UpdateEditorModel(Services.ContentManager.New<Page>("page"), this);
            if (!publishNow && publishDate != null)
                model.Page.Item.Published = publishDate.Value;

            if (!ModelState.IsValid) {
                Services.TransactionManager.Cancel();
                return View(model);
            }

            Services.ContentManager.Create(model.Page.Item.ContentItem, VersionOptions.Draft);

            if (publishNow)
                Services.ContentManager.Publish(model.Page.Item.ContentItem);

            if (publishNow)
                Services.Notifier.Information(T("Page has been published"));
            else if (publishDate != null)
                Services.Notifier.Information(T("Page has been scheduled for publishing"));
            else
                Services.Notifier.Information(T("Page draft has been saved"));

            return RedirectToAction("Edit", "Admin", new { id = model.Page.Item.ContentItem.Id });
        }

        public ActionResult Edit(int id) {
            Page page = _pageService.GetLatest(id);
            if (page == null)
                return new NotFoundResult();

            if (!Services.Authorizer.Authorize(Permissions.EditOthersPages, page, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();

            var model = new PageEditViewModel {
                Page = Services.ContentManager.BuildEditorModel(page)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id) {
            Page page = _pageService.GetPageOrDraft(id);
            if (page == null)
                return new NotFoundResult();

            if (!Services.Authorizer.Authorize(Permissions.EditOthersPages, page, T("Couldn't edit page")))
                return new HttpUnauthorizedResult();


            //TODO: (erikpo) Move this duplicate code somewhere else
            DateTime? publishDate = null;
            bool publishNow = false;
            if (string.Equals(Request.Form["Command"], "PublishNow")) {
                publishNow = true;
            }
            else if (string.Equals(Request.Form["Command"], "PublishLater")) {
                DateTime publishDateValue;
                if (DateTime.TryParse(Request.Form["Published"], out publishDateValue)) {
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

            if (publishNow)
                Services.Notifier.Information(T("Page has been published"));
            else if (publishDate != null)
                Services.Notifier.Information(T("Page has been scheduled for publishing"));
            else
                Services.Notifier.Information(T("Page draft has been saved"));

            return RedirectToAction("Edit", "Admin", new { id = model.Page.Item.ContentItem.Id });
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            Page page = _pageService.Get(id);
            if (page == null)
                return new NotFoundResult();

            if (!Services.Authorizer.Authorize(Permissions.DeleteOthersPages, page, T("Couldn't delete page")))
                return new HttpUnauthorizedResult();

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