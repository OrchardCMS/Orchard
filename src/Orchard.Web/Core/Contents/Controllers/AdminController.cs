using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Contents.ViewModels;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Mvc.Html;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Settings;
using Orchard.Utility.Extensions;
using Orchard.Localization.Services;

namespace Orchard.Core.Contents.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITransactionManager _transactionManager;
        private readonly ISiteService _siteService;
        private readonly ICultureManager _cultureManager;
        private readonly ICultureFilter _cultureFilter;

        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITransactionManager transactionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            ICultureManager cultureManager,
            ICultureFilter cultureFilter) {
            Services = orchardServices;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            _siteService = siteService;
            _cultureManager = cultureManager;
            _cultureFilter = cultureFilter;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult List(ListContentsViewModel model, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var versionOptions = VersionOptions.Latest;
            switch (model.Options.ContentsStatus) {
                case ContentsStatus.Published:
                    versionOptions = VersionOptions.Published;
                    break;
                case ContentsStatus.Draft:
                    versionOptions = VersionOptions.Draft;
                    break;
                case ContentsStatus.AllVersions:
                    versionOptions = VersionOptions.AllVersions;
                    break;
                default:
                    versionOptions = VersionOptions.Latest;
                    break;
            }

            var query = _contentManager.Query(versionOptions, GetListableTypes(false).Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.TypeName)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;

                // We display a specific type even if it's not listable so that admin pages
                // can reuse the Content list page for specific types.
                query = _contentManager.Query(versionOptions, model.TypeName);
            }

            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    //query = query.OrderByDescending<ContentPartRecord, int>(ci => ci.ContentItemRecord.Versions.Single(civr => civr.Latest).Id);
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    //query = query.OrderByDescending<ContentPartRecord, int>(ci => ci.Id);
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            if (!String.IsNullOrWhiteSpace(model.Options.SelectedCulture)) {
                query = _cultureFilter.FilterCulture(query, model.Options.SelectedCulture);
            }

            if (model.Options.ContentsStatus == ContentsStatus.Owner) {
                query = query.Where<CommonPartRecord>(cr => cr.OwnerId == Services.WorkContext.CurrentUser.Id);
            }

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = GetListableTypes(false)
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            model.Options.Cultures = _cultureManager.ListCultures();

            var maxPagedCount = _siteService.GetSiteSettings().MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;
            var pagerShape = Shape.Pager(pager).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            var viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            return View(viewModel);
        }

        private IEnumerable<ContentTypeDefinition> GetCreatableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                Services.Authorizer.Authorize(Permissions.EditContent, _contentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Creatable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

        private IEnumerable<ContentTypeDefinition> GetListableTypes(bool andContainable) {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd =>
                Services.Authorizer.Authorize(Permissions.EditContent, _contentManager.New(ctd.Name)) &&
                ctd.Settings.GetModel<ContentTypeSettings>().Listable &&
                (!andContainable || ctd.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")));
        }

        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.SelectedCulture"] = options.SelectedCulture; //todo: don't hard-code the key
                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
                if (GetListableTypes(false).Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                    routeValues["id"] = options.SelectedFilter;
                }
                else {
                    routeValues.Remove("id");
                }
            }

            return RedirectToAction("List", routeValues);
        }

        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl) {
            if (itemIds != null) {
                var checkedContentItems = _contentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
                switch (options.BulkAction) {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.PublishNow:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't publish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Publish(item);
                        }
                        Services.Notifier.Success(T("Content successfully published."));
                        break;
                    case ContentsBulkAction.Unpublish:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Unpublish(item);
                        }
                        Services.Notifier.Success(T("Content successfully unpublished."));
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Remove(item);
                        }
                        Services.Notifier.Success(T("Content successfully removed."));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        ActionResult CreatableTypeList(int? containerId) {
            var viewModel = Shape.ViewModel(ContentTypes: GetCreatableTypes(containerId.HasValue), ContainerId: containerId);

            return View("CreatableTypeList", viewModel);
        }

        ActionResult ListableTypeList(int? containerId) {
            var viewModel = Shape.ViewModel(ContentTypes: GetListableTypes(containerId.HasValue), ContainerId: containerId);

            return View("ListableTypeList", viewModel);
        }

        public ActionResult Create(string id, int? containerId) {
            if (string.IsNullOrEmpty(id))
                return CreatableTypeList(containerId);

            if (_contentDefinitionManager.GetTypeDefinition(id) == null) {
                return RedirectToAction("Create", new { id = "" });
            }

            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            if (containerId.HasValue && contentItem.Is<ContainablePart>()) {
                var common = contentItem.As<CommonPart>();
                if (common != null) {
                    common.Container = _contentManager.Get(containerId.Value);
                }
            }

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult CreatePOST(string id, string returnUrl) {
            return CreatePOST(id, returnUrl, contentItem => { return false; });
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult CreateAndPublishPOST(string id, string returnUrl) {

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, dummyContent, T("You do not have permission to publish content.")))
                return new HttpUnauthorizedResult();

            return CreatePOST(id, returnUrl, contentItem => {
                _contentManager.Publish(contentItem);
                return true;
            });
        }

        private ActionResult CreatePOST(string id, string returnUrl, Func<ContentItem, bool> conditionallyPublish) {
            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("You do not have permission to edit content.")))
                return new HttpUnauthorizedResult();

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = _contentManager.UpdateEditor(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            var contentWasPublished = conditionallyPublish(contentItem);

            if (contentWasPublished) {
                Services.Notifier.Success(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("The content has been created and published.")
                    : T("The {0} has been created and published.", contentItem.TypeDefinition.DisplayName));
            }
            else {
                Services.Notifier.Success(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("The content has been created as a draft.")
                    : T("The {0} has been created as a draft.", contentItem.TypeDefinition.DisplayName));
            }

            if (!string.IsNullOrEmpty(returnUrl)) {
                return this.RedirectLocal(returnUrl);
            }

            var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        public ActionResult Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("You do not have permission to edit content.")))
                return new HttpUnauthorizedResult();

            var model = _contentManager.BuildEditor(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int id, string returnUrl) {
            return EditPOST(id, returnUrl, contentItem => { return false; });
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Publish")]
        public ActionResult EditAndPublishPOST(int id, string returnUrl) {
            var content = _contentManager.Get(id, VersionOptions.Latest);

            if (content == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, content, T("You do not have permission to publish content.")))
                return new HttpUnauthorizedResult();

            return EditPOST(id, returnUrl, contentItem => {
                _contentManager.Publish(contentItem);
                return true;
            });
        }

        private ActionResult EditPOST(int id, string returnUrl, Func<ContentItem, bool> conditionallyPublish) {
            var contentItem = _contentManager.Get(id, VersionOptions.DraftRequired);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("You do not have permission to edit content.")))
                return new HttpUnauthorizedResult();

            string previousRoute = null;
            if (contentItem.Has<IAliasAspect>()
                && !string.IsNullOrWhiteSpace(returnUrl)
                && Request.IsLocalUrl(returnUrl)
                // only if the original returnUrl is the content itself
                && String.Equals(returnUrl, Url.ItemDisplayUrl(contentItem), StringComparison.OrdinalIgnoreCase)
                ) {
                previousRoute = contentItem.As<IAliasAspect>().Path;
            }

            var model = _contentManager.UpdateEditor(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View("Edit", model);
            }

            var contentWasPublished = conditionallyPublish(contentItem);

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && previousRoute != null
                && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase)) {
                returnUrl = Url.ItemDisplayUrl(contentItem);
            }

            if (contentWasPublished) {
                Services.Notifier.Success(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("The content has been published.")
                    : T("The {0} has been published.", contentItem.TypeDefinition.DisplayName));
            }
            else {
                Services.Notifier.Success(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("The content has been saved as a draft.")
                    : T("The {0} has been saved as a draft.", contentItem.TypeDefinition.DisplayName));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } }));
        }

        [HttpPost]
        public ActionResult Clone(int id) {
            var originalContentItem = _contentManager.GetLatest(id);

            if (!Services.Authorizer.Authorize(Permissions.ViewContent, originalContentItem, T("You do not have permission to view existing content.")))
                return new HttpUnauthorizedResult();

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(originalContentItem.ContentType);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, dummyContent, T("You do not have permission to edit (or create) content.")))
                return new HttpUnauthorizedResult();

            var cloneContentItem = _contentManager.Clone(originalContentItem);

            Services.Notifier.Success(string.IsNullOrWhiteSpace(originalContentItem.TypeDefinition.DisplayName)
                ? T("The content has been cloned as a draft.")
                : T("The {0} has been cloned as a draft.", originalContentItem.TypeDefinition.DisplayName));

            var adminRouteValues = _contentManager.GetItemMetadata(cloneContentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        [HttpPost]
        public ActionResult Remove(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, contentItem, T("You do not have permission to delete content.")))
                return new HttpUnauthorizedResult();

            if (contentItem != null) {
                _contentManager.Remove(contentItem);
                Services.Notifier.Success(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("The content has been removed.")
                    : T("The {0} has been removed.", contentItem.TypeDefinition.DisplayName));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult DiscardDraft(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null) {
                return HttpNotFound();
            }

            if (!contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable
                || !contentItem.HasPublished()
                || contentItem.IsPublished()) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, contentItem, T("You do not have permission to delete content (or discard draft content)."))) {
                return new HttpUnauthorizedResult();
            }

            _contentManager.DiscardDraft(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("The draft content has been removed.")
                : T("The draft {0} has been removed.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Publish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("You do not have permission to publish content.")))
                return new HttpUnauthorizedResult();

            _contentManager.Publish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("The content has been published.")
                : T("The {0} has been published.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Unpublish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("You do not have permission to publish (or unpublish) content.")))
                return new HttpUnauthorizedResult();

            _contentManager.Unpublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("The content has been unpublished.")
                : T("The {0} has been unpublished.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

    [Obsolete("Use Orchard.Mvc.FormValueRequiredAttribute instead.")]
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
