﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

namespace Orchard.Core.Contents.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITransactionManager _transactionManager;
        private readonly ISiteService _siteService;

        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITransactionManager transactionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            Services = orchardServices;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _transactionManager = transactionManager;
            _siteService = siteService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public async Task<ActionResult> List(ListContentsViewModel model, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var versionOptions = VersionOptions.Latest;
            switch (model.Options.ContentsStatus)
            {
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

            var query = _contentManager.Query(versionOptions, GetCreatableTypes(false).Select(ctd => ctd.Name).ToArray());

            if (!string.IsNullOrEmpty(model.TypeName)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !string.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.TypeName);
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

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = GetCreatableTypes(false)
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            var shapeTasks = pageOfContentItems.Select(ci => _contentManager.BuildDisplayAsync(ci, "SummaryAdmin")).ToArray();

            await Task.WhenAll(shapeTasks);

            list.AddRange(shapeTasks.Select(task => task.Result));

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

        [HttpPost, ActionName("List")]
        [Mvc.FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
                routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
                if (GetCreatableTypes(false).Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
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
                        Services.Notifier.Information(T("Content successfully published."));
                        break;
                    case ContentsBulkAction.Unpublish:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Unpublish(item);
                        }
                        Services.Notifier.Information(T("Content successfully unpublished."));
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems) {
                            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }

                            _contentManager.Remove(item);
                        }
                        Services.Notifier.Information(T("Content successfully removed."));
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

        public async Task<ActionResult> Create(string id, int? containerId) {
            if (string.IsNullOrEmpty(id))
                return CreatableTypeList(containerId);

            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot create content")))
                return new HttpUnauthorizedResult();

            if (containerId.HasValue && contentItem.Is<ContainablePart>()) {
                var common = contentItem.As<CommonPart>();
                if (common != null) {
                    common.Container = _contentManager.Get(containerId.Value);
                }
            }

            var model = await _contentManager.BuildEditorAsync(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Save")]
        public Task<ActionResult> CreatePOST(string id, string returnUrl) {
            return CreatePOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Create")]
        [Mvc.FormValueRequired("submit.Publish")]
        public async Task<ActionResult> CreateAndPublishPOST(string id, string returnUrl) {

            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, dummyContent, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            return await CreatePOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private async Task<ActionResult> CreatePOST(string id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.New(id);

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't create content")))
                return new HttpUnauthorizedResult();

            _contentManager.Create(contentItem, VersionOptions.Draft);

            var model = await _contentManager.UpdateEditorAsync(contentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            conditionallyPublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been created.")
                : T("Your {0} has been created.", contentItem.TypeDefinition.DisplayName));
            if (!string.IsNullOrEmpty(returnUrl)) {
                return this.RedirectLocal(returnUrl);
            }
            var adminRouteValues = _contentManager.GetItemMetadata(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        public async Task<ActionResult> Edit(int id) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Cannot edit content")))
                return new HttpUnauthorizedResult();

            var model = await _contentManager.BuildEditorAsync(contentItem);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Save")]
        public Task<ActionResult> EditPOST(int id, string returnUrl) {
            return EditPOST(id, returnUrl, contentItem => {
                if (!contentItem.Has<IPublishingControlAspect>() && !contentItem.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                    _contentManager.Publish(contentItem);
            });
        }

        [HttpPost, ActionName("Edit")]
        [Mvc.FormValueRequired("submit.Publish")]
        public async Task<ActionResult> EditAndPublishPOST(int id, string returnUrl) {
            var content = _contentManager.Get(id, VersionOptions.Latest);

            if (content == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, content, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            return await EditPOST(id, returnUrl, contentItem => _contentManager.Publish(contentItem));
        }

        private async Task<ActionResult> EditPOST(int id, string returnUrl, Action<ContentItem> conditionallyPublish) {
            var contentItem = _contentManager.Get(id, VersionOptions.DraftRequired);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't edit content")))
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

            var model = await _contentManager.UpdateEditorAsync(contentItem, this);
            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View("Edit", model);
            }

            conditionallyPublish(contentItem);

            if (!string.IsNullOrWhiteSpace(returnUrl)
                && previousRoute != null
                && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase)) {
                returnUrl = Url.ItemDisplayUrl(contentItem);
            }

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                ? T("Your content has been saved.")
                : T("Your {0} has been saved.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Edit", new RouteValueDictionary { { "Id", contentItem.Id } }));
        }

        [HttpPost]
        public ActionResult Clone(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);

            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.EditContent, contentItem, T("Couldn't clone content")))
                return new HttpUnauthorizedResult();

            try {
                Services.ContentManager.Clone(contentItem);
            }
            catch (InvalidOperationException) {
                Services.Notifier.Warning(T("Could not clone the content item."));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
            }

            Services.Notifier.Information(T("Successfully cloned. The clone was saved as a draft."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Remove(int id, string returnUrl) {
            var contentItem = _contentManager.Get(id, VersionOptions.Latest);

            if (!Services.Authorizer.Authorize(Permissions.DeleteContent, contentItem, T("Couldn't remove content")))
                return new HttpUnauthorizedResult();

            if (contentItem != null) {
                _contentManager.Remove(contentItem);
                Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName)
                    ? T("That content has been removed.")
                    : T("That {0} has been removed.", contentItem.TypeDefinition.DisplayName));
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Publish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't publish content")))
                return new HttpUnauthorizedResult();

            _contentManager.Publish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been published.") : T("That {0} has been published.", contentItem.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost]
        public ActionResult Unpublish(int id, string returnUrl) {
            var contentItem = _contentManager.GetLatest(id);
            if (contentItem == null)
                return HttpNotFound();

            if (!Services.Authorizer.Authorize(Permissions.PublishContent, contentItem, T("Couldn't unpublish content")))
                return new HttpUnauthorizedResult();

            _contentManager.Unpublish(contentItem);

            Services.Notifier.Information(string.IsNullOrWhiteSpace(contentItem.TypeDefinition.DisplayName) ? T("That content has been unpublished.") : T("That {0} has been unpublished.", contentItem.TypeDefinition.DisplayName));

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
