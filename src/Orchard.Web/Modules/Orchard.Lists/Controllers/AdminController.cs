using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.ViewModels;
using Orchard.Core.Contents;
using Orchard.Core.Contents.ViewModels;
using Orchard.Core.Title.Models;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Lists.Helpers;
using Orchard.Lists.ViewModels;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using ContentOptions = Orchard.Lists.ViewModels.ContentOptions;
using ContentsBulkAction = Orchard.Lists.ViewModels.ContentsBulkAction;
using ListContentsViewModel = Orchard.Lists.ViewModels.ListContentsViewModel;

namespace Orchard.Lists.Controllers {
    public class AdminController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _services;
        private readonly IContainerService _containerService;
        private readonly IListViewService _listViewService;
        private readonly ITransactionManager _transactionManager;

        public AdminController(
            IOrchardServices services,
            IContentDefinitionManager contentDefinitionManager,
            IShapeFactory shapeFactory,
            IContainerService containerService, 
            IListViewService listViewService,
            ITransactionManager transactionManager) {

            _services = services;
            _contentManager = services.ContentManager;
            _contentDefinitionManager = contentDefinitionManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            _containerService = containerService;
            _listViewService = listViewService;
            _transactionManager = transactionManager;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        public ActionResult Index(Core.Contents.ViewModels.ListContentsViewModel model, PagerParameters pagerParameters) {
            var query = _containerService.GetContainersQuery(VersionOptions.Latest);

            if (!String.IsNullOrEmpty(model.TypeName)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return HttpNotFound();

                model.TypeDisplayName = !String.IsNullOrWhiteSpace(contentTypeDefinition.DisplayName)
                                            ? contentTypeDefinition.DisplayName
                                            : contentTypeDefinition.Name;
                query = query.ForType(model.TypeName);
            }

            switch (model.Options.OrderBy) {
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

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = _containerService.GetContainerTypes()
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfLists = query.Slice(pager.GetStartIndex(), pager.PageSize);
            
            var listsShape = Shape.List();
            listsShape.AddRange(pageOfLists.Select(x => _contentManager.BuildDisplay(x, "SummaryAdmin")).ToList());
            var viewModel = Shape.ViewModel()
                .Lists(listsShape)
                .Pager(pagerShape)
                .Options(model.Options);
            return View(viewModel);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.OrderBy"] = options.OrderBy;
                if (_containerService.GetContainerTypes().Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase))) {
                    routeValues["id"] = options.SelectedFilter;
                }
                else {
                    routeValues.Remove("id");
                }
            }

            return RedirectToAction("Index", routeValues);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, PagerParameters pagerParameters) {
            if (itemIds != null) {
                var checkedContentItems = _contentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
                switch (options.BulkAction) {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.PublishNow:
                        foreach (var item in checkedContentItems) {
                            if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, item, T("Couldn't publish selected lists."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }
                            _contentManager.Publish(item);
                        }
                        _services.Notifier.Success(T("Lists successfully published."));
                        break;
                    case ContentsBulkAction.Unpublish:
                        foreach (var item in checkedContentItems) {
                            if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, item, T("Couldn't unpublish selected lists."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }
                            _contentManager.Unpublish(item);
                        }
                        _services.Notifier.Success(T("Lists successfully unpublished."));
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems) {
                            if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.DeleteContent, item, T("Couldn't remove selected lists."))) {
                                _transactionManager.Cancel();
                                return new HttpUnauthorizedResult();
                            }
                            _contentManager.Remove(item);
                        }
                        _services.Notifier.Success(T("Lists successfully removed."));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction("Index", new { page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        public ActionResult Create(string id) {
            if (String.IsNullOrWhiteSpace(id)) {
                var containerTypes = _containerService.GetContainerTypes().ToList();
                if (containerTypes.Count > 1) {
                    return RedirectToAction("SelectType");
                }
                return RedirectToAction("Create", new {id = containerTypes.First().Name});
            }

            return RedirectToAction("Create", "Admin", new {area = "Contents", id, returnUrl = Url.Action("Index", "Admin", new { area = "Orchard.Lists" })});
        }

        public ActionResult SelectType() {
            var viewModel = Shape.ViewModel().ContainerTypes(_containerService.GetContainerTypes().ToList());
            return View(viewModel);
        }

        public ActionResult List(ListContentsViewModel model, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var container = _contentManager.GetLatest(model.ContainerId);
            if (container == null || !container.Has<ContainerPart>()) {
                return HttpNotFound();
            }

            model.ContainerDisplayName = container.ContentManager.GetItemMetadata(container).DisplayText;
            if (string.IsNullOrEmpty(model.ContainerDisplayName)) {
                model.ContainerDisplayName = container.ContentType;
            }

            var query = GetListContentItemQuery(model.ContainerId);
            if (query == null) {
                return HttpNotFound();
            }

            var containerPart = container.As<ContainerPart>();
            if (containerPart.EnablePositioning) {
                query = OrderByPosition(query);
            }
            else {
                switch (model.Options.OrderBy) {
                    case SortBy.Modified:
                        query = query.OrderByDescending<CommonPartRecord>(cr => cr.ModifiedUtc);
                        break;
                    case SortBy.Published:
                        query = query.OrderByDescending<CommonPartRecord>(cr => cr.PublishedUtc);
                        break;
                    case SortBy.Created:
                        query = query.OrderByDescending<CommonPartRecord>(cr => cr.CreatedUtc);
                        break;
                    case SortBy.DisplayText:
                        // Note: This will obviously not work for items without a TitlePart, but we're OK with that.
                        query = query.OrderBy<TitlePartRecord>(cr => cr.Title);
                        break;
                }
            }

            var listView = containerPart.AdminListView.BuildDisplay(new BuildListViewDisplayContext {
                New = _services.New,
                Container = containerPart,
                ContentQuery = query,
                Pager = pager,
                ContainerDisplayName = model.ContainerDisplayName
            });

            var viewModel = Shape.ViewModel()
                .Pager(pager)
                .ListView(listView)
                .ListViewProvider(containerPart.AdminListView)
                .ListViewProviders(_listViewService.Providers.ToList())
                .Options(model.Options)
                .Container(container)
                .ContainerId(model.ContainerId)
                .ContainerDisplayName(model.ContainerDisplayName)
                .ContainerContentType(container.ContentType)
                .ItemContentTypes(container.As<ContainerPart>().ItemContentTypes.ToList())
                ;

            if (containerPart.Is<ContainablePart>()) {
                viewModel.ListNavigation(_services.New.ListNavigation(ContainablePart: containerPart.As<ContainablePart>()));
            }

            return View(viewModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Order")]
        public ActionResult ListOrderPOST(ContentOptions options) {
            var routeValues = ControllerContext.RouteData.Values;
            if (options != null) {
                routeValues["Options.OrderBy"] = options.OrderBy;
            }
            return RedirectToAction("List", routeValues);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, int? targetContainerId, PagerParameters pagerParameters, string returnUrl) {
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

            return this.RedirectLocal(returnUrl, () => RedirectToAction("List", new { page = pagerParameters.Page, pageSize = pagerParameters.PageSize }));
        }

        [HttpPost]
        public ActionResult Insert(int containerId, int itemId, PagerParameters pagerParameters) {
            var container = _containerService.Get(containerId, VersionOptions.Latest);
            var item = _contentManager.Get(itemId, VersionOptions.Latest, new QueryHints().ExpandParts<CommonPart, ContainablePart>());
            var commonPart = item.As<CommonPart>();
            var previousItemContainer = commonPart.Container;
            var itemMetadata = _contentManager.GetItemMetadata(item);
            var containerMetadata = _contentManager.GetItemMetadata(container);
            var position = _containerService.GetFirstPosition(containerId) + 1;
            LocalizedString message;

            if (previousItemContainer == null) {
                message = T("{0} was moved to <a href=\"{1}\">{2}</a>", itemMetadata.DisplayText, Url.RouteUrl(containerMetadata.AdminRouteValues), containerMetadata.DisplayText);
            }
            else if (previousItemContainer.Id != containerId) {
                var previousItemContainerMetadata = _contentManager.GetItemMetadata(commonPart.Container);
                message = T("{0} was moved from <a href=\"{3}\">{4}</a> to <a href=\"{1}\">{2}</a>",
                    itemMetadata.DisplayText,
                    Url.RouteUrl(containerMetadata.AdminRouteValues),
                    containerMetadata.DisplayText,
                    Url.RouteUrl(previousItemContainerMetadata.AdminRouteValues),
                    previousItemContainerMetadata.DisplayText);
            }
            else {
                message = T("{0} is already part of this list and was moved to the top.", itemMetadata.DisplayText);
            }

            _containerService.MoveItem(item.As<ContainablePart>(), container, position);
            _services.Notifier.Information(message);
            return RedirectToAction("List", new { containerId, page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        [HttpPost]
        public ActionResult UpdatePositions(int containerId, int oldIndex, int newIndex, PagerParameters pagerParameters) {
            var pager = new Pager(_services.WorkContext.CurrentSite, pagerParameters);
            var query = OrderByPosition(GetListContentItemQuery(containerId));
            if (query == null) {
                return HttpNotFound();
            }
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            var contentItem = pageOfContentItems[oldIndex];
            pageOfContentItems.Remove(contentItem);
            pageOfContentItems.Insert(newIndex, contentItem);

            var index = pager.GetStartIndex() + pageOfContentItems.Count;
            foreach (var item in pageOfContentItems.Select(x => x.As<ContainablePart>())) {
                item.Position = --index;
                RePublish(item);
            }
            return new EmptyResult();
        }

        [ActionName("List")]
        [HttpPost, FormValueRequired("submit.ListOp")]
        public ActionResult ListOperation(int containerId, ListOperation operation, SortBy? sortBy, SortDirection? sortByDirection, PagerParameters pagerParameters) {
            var items = _containerService.GetContentItems(containerId, VersionOptions.Latest).Select(x => x.As<ContainablePart>());
            switch (operation) {
                case ViewModels.ListOperation.Reverse:
                    _containerService.Reverse(items);
                    _services.Notifier.Success(T("The list has been reversed."));
                    break;
                case ViewModels.ListOperation.Shuffle:
                    _containerService.Shuffle(items);
                    _services.Notifier.Success(T("The list has been shuffled."));
                    break;
                case ViewModels.ListOperation.Sort:
                    _containerService.Sort(items, sortBy.GetValueOrDefault(), sortByDirection.GetValueOrDefault());
                    _services.Notifier.Success(T("The list has been sorted."));
                    break;
                default:
                    _services.Notifier.Error(T("Please select an operation to perform on the list."));
                    break;
            }

            return RedirectToAction("List", new {containerId, page = pagerParameters.Page, pageSize = pagerParameters.PageSize});
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("listViewName")]
        public ActionResult ChangeListView(int containerId, string listViewName, PagerParameters pagerParameters) {
            var container = _containerService.Get(containerId, VersionOptions.Latest);
            if (container == null || !container.Has<ContainerPart>()) {
                return HttpNotFound();
            }

            container.Record.AdminListViewName = listViewName;
            return RedirectToAction("List", new { containerId, page = pagerParameters.Page, pageSize = pagerParameters.PageSize });
        }

        /// <summary>
        /// Only publishes the content if it is already published.
        /// </summary>
        private void RePublish(IContent content) {
            if(content.ContentItem.VersionRecord.Published)
                _contentManager.Publish(content.ContentItem);
        }

        private IContentQuery<ContentItem> GetListContentItemQuery(int containerId) {
            var containableTypes = GetContainableTypes().Select(ctd => ctd.Name).ToList();
            if (containableTypes.Count == 0) {
                // Force the name to be matched against empty and return no items in the query
                containableTypes.Add(string.Empty);
            }

            var query = _contentManager
                .Query(VersionOptions.Latest, containableTypes.ToArray())
                .Join<CommonPartRecord>().Where(cr => cr.Container.Id == containerId);

            return query;
        }

        private IContentQuery<ContentItem> OrderByPosition(IContentQuery<ContentItem> query) {
            return query.Join<ContainablePartRecord>().OrderByDescending(x => x.Position);
        }

        private IEnumerable<ContentTypeDefinition> GetContainableTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Parts.Any(c => c.PartDefinition.Name == "ContainablePart"));
        }

        private bool BulkMoveToList(IEnumerable<int> selectedIds, int? targetContainerId) {
            if (!targetContainerId.HasValue) {
                _services.Notifier.Information(T("Please select the list to move the items to."));
                return true;
            }
            var id = targetContainerId.Value;
            var targetContainer = _contentManager.Get<ContainerPart>(id);
            if (targetContainer == null) {
                _services.Notifier.Information(T("Please select the list to move the items to."));
                return true;
            }

            var itemContentTypes = targetContainer.ItemContentTypes.ToList();
            var containerDisplayText = _contentManager.GetItemMetadata(targetContainer).DisplayText ?? targetContainer.ContentItem.ContentType;
            var selectedItems = _contentManager.GetMany<ContainablePart>(selectedIds, VersionOptions.Latest, QueryHints.Empty);

            foreach (var item in selectedItems) {
                if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, item, T("Couldn't move selected content."))) {
                    return false;
                }
                
                // Ensure the item can be in that container.
                if (itemContentTypes.Any() && itemContentTypes.All(x => x.Name != item.ContentItem.ContentType)) {
                    _services.TransactionManager.Cancel();
                    _services.Notifier.Warning(T("One or more items could not be moved to '{0}' because it is restricted to containing items of type '{1}'.", containerDisplayText, itemContentTypes.Select(x => x.DisplayName).ToOrString(T)));
                    return true; // todo: transactions
                }

                _containerService.MoveItem(item, targetContainer);
            }
            _services.Notifier.Success(T("Content successfully moved to <a href=\"{0}\">{1}</a>.", Url.Action("List", new { containerId = targetContainerId }), containerDisplayText));
            return true;
        }

        private bool BulkRemoveFromList(IEnumerable<int> itemIds) {
            var selectedItems = _contentManager.GetMany<ContainablePart>(itemIds, VersionOptions.Latest, QueryHints.Empty);
            foreach (var item in selectedItems) {
                if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.EditContent, item, T("Couldn't remove selected content from the list."))) {
                    _services.TransactionManager.Cancel();
                    return false;
                }
                item.As<CommonPart>().Record.Container = null;
                _containerService.UpdateItemPath(item.ContentItem);
            }
            _services.Notifier.Success(T("Content successfully removed from the list."));
            return true;
        }

        private bool BulkRemove(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
                    _services.TransactionManager.Cancel();
                    return false;
                }

                _contentManager.Remove(item);
            }
            _services.Notifier.Success(T("Content successfully removed."));
            return true;
        }

        private bool BulkUnpublish(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, item, T("Couldn't unpublish selected content."))) {
                    _services.TransactionManager.Cancel();
                    return false;
                }

                _contentManager.Unpublish(item);
            }
            _services.Notifier.Success(T("Content successfully unpublished."));
            return true;
        }

        private bool BulkPublishNow(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!_services.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, item, T("Couldn't publish selected content."))) {
                    _services.TransactionManager.Cancel();
                    return false;
                }

                _contentManager.Publish(item);
            }
            _services.Notifier.Success(T("Content successfully published."));
            return true;
        }
    }
}
