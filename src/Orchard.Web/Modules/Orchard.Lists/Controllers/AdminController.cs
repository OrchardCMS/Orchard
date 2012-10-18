using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.DisplayManagement;
using Orchard.Lists.ViewModels;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Orchard.Lists.Controllers {
    public class AdminController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;

        public IOrchardServices Services { get; set; }

        public AdminController(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            IShapeFactory shapeFactory) {

            Services = orchardServices;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _siteService = siteService;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        private IEnumerable<ContentTypeDefinition> GetContainableTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Parts.Any(c => c.PartDefinition.Name == "ContainablePart") && ctd.Settings.GetModel<ContentTypeSettings>().Creatable);
        }

        public ActionResult List(ListContentsViewModel model, PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var container = model.ContainerId.HasValue ? _contentManager.GetLatest((int)model.ContainerId) : null;
            if (container == null || !container.Has<ContainerPart>()) {
                return HttpNotFound();
            }

            var restrictedContentType = container.As<ContainerPart>().Record.ItemContentType;
            var hasRestriction = !string.IsNullOrEmpty(restrictedContentType);
            if (hasRestriction) {
                model.FilterByContentType = restrictedContentType;
            }
            model.Options.SelectedFilter = model.FilterByContentType;

            model.ContainerDisplayName = container.ContentManager.GetItemMetadata(container).DisplayText;
            if (string.IsNullOrEmpty(model.ContainerDisplayName)) {
                model.ContainerDisplayName = container.ContentType;
            }

            var query = GetListContentItemQuery(model.ContainerId.Value, model.FilterByContentType, model.Options.OrderBy);
            if (query == null) {
                return HttpNotFound();
            }

            model.Options.FilterOptions = GetContainableTypes()
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Key);

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            var containerItemContentDisplayName = String.Empty;
            if (hasRestriction)
                containerItemContentDisplayName = _contentDefinitionManager.GetTypeDefinition(restrictedContentType).DisplayName;
            else if (!string.IsNullOrEmpty(model.FilterByContentType))
                containerItemContentDisplayName = _contentDefinitionManager.GetTypeDefinition(model.FilterByContentType).DisplayName;

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .ContainerId(model.ContainerId)
                .Options(model.Options)
                .ContainerDisplayName(model.ContainerDisplayName)
                .HasRestriction(hasRestriction)
                .ContainerContentType(container.ContentType)
                .ContainerItemContentDisplayName(containerItemContentDisplayName)
                .ContainerItemContentType(hasRestriction ? restrictedContentType : (model.FilterByContentType ?? ""))
                .OtherLists(_contentManager.Query<ContainerPart>(VersionOptions.Latest).List()
                    .Select(part => part.ContentItem)
                    .Where(item => item != container)
                    .OrderBy(item => item.As<CommonPart>().VersionPublishedUtc));

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        private IContentQuery<ContentItem> GetListContentItemQuery(int containerId, string contentType, ContentsOrder orderBy) {
            List<string> containableTypes = GetContainableTypes().Select(ctd => ctd.Name).ToList();
            if (containableTypes.Count == 0) {
                // Force the name to be matched against empty and return no items in the query
                containableTypes.Add(string.Empty);
            }

            var query = _contentManager.Query(VersionOptions.Latest, containableTypes.ToArray());
            
            if (!string.IsNullOrEmpty(contentType)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentTypeDefinition == null) {
                    return null;
                }
                query = query.ForType(contentType);
            }

            query = containerId == 0
                ? query.Join<CommonPartRecord>().Where(cr => cr.Container == null)
                : query.Join<CommonPartRecord>().Where(cr => cr.Container.Id == containerId);
            switch (orderBy) {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                    break;
            }

            return query;
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, int? targetContainerId, string returnUrl) {
            if (itemIds != null) {
                switch (options.BulkAction) {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.PublishNow:
                        if (!BulkPublishNow(itemIds)) {
                            return new HttpUnauthorizedResult();
                        }
                        break;
                    case ContentsBulkAction.Unpublish:
                        if (!BulkUnpublish(itemIds)) {
                            return new HttpUnauthorizedResult();
                        }
                        break;
                    case ContentsBulkAction.Remove:
                        if (!BulkRemove(itemIds)) {
                            return new HttpUnauthorizedResult();
                        }
                        break;
                    case ContentsBulkAction.RemoveFromList:
                        if (!BulkRemoveFromList(itemIds)) {
                            return new HttpUnauthorizedResult();
                        }
                        break;
                    case ContentsBulkAction.MoveToList:
                        if (!BulkMoveToList(itemIds, targetContainerId)) {
                            return new HttpUnauthorizedResult();
                        }
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.OrderBy"] = options.OrderBy;
                if (GetContainableTypes().Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                    routeValues["filterByContentType"] = options.SelectedFilter;
                }
                else {
                    routeValues.Remove("filterByContentType");
                }
            }

            return RedirectToAction("List", routeValues);
        }

        public ActionResult Choose(ChooseContentsViewModel model, PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var container = model.SourceContainerId == 0 ? null : _contentManager.GetLatest(model.SourceContainerId);
            if (container == null && model.SourceContainerId != 0) {
                return HttpNotFound();
            }
            if (string.IsNullOrEmpty(model.FilterByContentType)) {
                var targetContainer = _contentManager.Get<ContainerPart>(model.TargetContainerId);
                if (targetContainer != null) {
                    model.FilterByContentType = targetContainer.Record.ItemContentType;
                }
            }

            var query = GetListContentItemQuery(model.SourceContainerId, model.FilterByContentType, model.OrderBy);
            if (query == null) {
                return HttpNotFound();
            }

            model.SelectedFilter = model.FilterByContentType;

            model.FilterOptions = GetContainableTypes()
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Key);

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .SourceContainerId(model.SourceContainerId)
                .TargetContainerId(model.TargetContainerId)
                .SelectedFilter(model.SelectedFilter)
                .FilterOptions(model.FilterOptions)
                .OrderBy(model.OrderBy)
                .Containers(_contentManager.Query<ContainerPart>(VersionOptions.Latest).List()
                    .Select(part => part.ContentItem)
                    .OrderBy(item => item.As<CommonPart>().VersionPublishedUtc));

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        [HttpPost, ActionName("Choose")]
        [FormValueRequired("submit.MoveTo")]
        public ActionResult ChoosePOST(IEnumerable<int> itemIds, int targetContainerId, string returnUrl) {
            if (itemIds != null && !BulkMoveToList(itemIds, targetContainerId)) {
                return new HttpUnauthorizedResult();
            }

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List", new { ContainerId = targetContainerId }));
        }

        [HttpPost, ActionName("Choose")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ChooseFilterPOST(ChooseContentsViewModel model) {
            var routeValues = ControllerContext.RouteData.Values;
            if (GetContainableTypes().Any(ctd => string.Equals(ctd.Name, model.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                routeValues["filterByContentType"] = model.SelectedFilter;
            }
            else {
                routeValues.Remove("filterByContentType");
            }
            if (model.SourceContainerId == 0) {
                routeValues.Remove("SourceContainerId");
            }
            else {
                routeValues["SourceContainerId"] = model.SourceContainerId;
            }
            routeValues["OrderBy"] = model.OrderBy;
            routeValues["TargetContainerId"] = model.TargetContainerId;

            return RedirectToAction("Choose", routeValues);
        }

        private void FixItemPath(ContentItem item) {
            // Fixes an Item's path when its container changes.

            // force a publish/unpublish event so RoutePart fixes the content items path
            // and the paths of any child objects if it is also a container.
            if (item.VersionRecord.Published) {
                item.VersionRecord.Published = false;
                _contentManager.Publish(item);
            }
            else {
                item.VersionRecord.Published = true;
                _contentManager.Unpublish(item);
            }
        }

        private bool BulkMoveToList(IEnumerable<int> itemIds, int? targetContainerId) {
            if (!targetContainerId.HasValue) {
                Services.Notifier.Information(T("Please select the list to move the items to."));
                return true;
            }
            var id = targetContainerId.Value;
            var targetContainer = _contentManager.Get<ContainerPart>(id);
            if (targetContainer == null) {
                Services.Notifier.Information(T("Please select the list to move the items to."));
                return true;
            }
            
            var itemContentType = targetContainer.Record.ItemContentType;

            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.EditContent, item, T("Couldn't move selected content."))) {
                    return false;
                }
                // ensure the item can be in that container.
                if (!string.IsNullOrEmpty(itemContentType) && item.ContentType != itemContentType) {
                    Services.TransactionManager.Cancel();
                    Services.Notifier.Information(T("One or more items could not be moved to '{0}' because it is restricted to containing items of type '{1}'.", _contentManager.GetItemMetadata(targetContainer).DisplayText ?? targetContainer.ContentItem.ContentType, itemContentType));
                    return true; // todo: transactions
                }

                item.As<CommonPart>().Record.Container = targetContainer.ContentItem.Record;
                FixItemPath(item);
            }
            Services.Notifier.Information(T("Content successfully moved to <a href=\"{0}\">{1}</a>.",
                                            Url.Action("List", new { containerId = targetContainerId }), _contentManager.GetItemMetadata(targetContainer).DisplayText ?? targetContainer.ContentItem.ContentType));
            return true;
        }

        private bool BulkRemoveFromList(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.EditContent, item, T("Couldn't remove selected content from the list."))) {
                    Services.TransactionManager.Cancel();
                    return false;
                }
                item.As<CommonPart>().Record.Container = null;
                FixItemPath(item);
            }
            Services.Notifier.Information(T("Content successfully removed from the list."));
            return true;
        }

        private bool BulkRemove(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                    Services.TransactionManager.Cancel();
                    return false;
                }

                _contentManager.Remove(item);
            }
            Services.Notifier.Information(T("Content successfully removed."));
            return true;
        }

        private bool BulkUnpublish(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                    Services.TransactionManager.Cancel();
                    return false;
                }

                _contentManager.Unpublish(item);
            }
            Services.Notifier.Information(T("Content successfully unpublished."));
            return true;
        }

        private bool BulkPublishNow(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't publish selected content."))) {
                    Services.TransactionManager.Cancel();
                    return false;
                }

                _contentManager.Publish(item);
            }
            Services.Notifier.Information(T("Content successfully published."));
            return true;
        }

    }
}
