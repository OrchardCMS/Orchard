using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Contents;
using Orchard.Core.Contents.Controllers;
using Orchard.Core.Contents.Settings;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using Orchard.Lists.ViewModels;
using Orchard.Localization;
using Orchard;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace Lists.Controllers {
    public class AdminController : Controller {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITransactionManager _transactionManager;
        private readonly ISiteService _siteService;

        public IOrchardServices Services { get; set; }

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

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }

        private IEnumerable<ContentTypeDefinition> GetContainableTypes() {
            return _contentDefinitionManager.ListTypeDefinitions().Where(ctd => ctd.Parts.Any(c => c.PartDefinition.Name == "ContainablePart") && ctd.Settings.GetModel<ContentTypeSettings>().Creatable);
        }

        public ActionResult List(ListContentsViewModel model, PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var container = model.ContainerId.HasValue ? _contentManager.GetLatest((int)model.ContainerId) : null;
            if (container == null && model.ContainerId.HasValue) {
                return HttpNotFound();
            }
            var restrictedContentType = container == null ? null : container.As<ContainerPart>().Record.ItemContentType;
            var hasRestriction = !string.IsNullOrEmpty(restrictedContentType);
            if (hasRestriction) {
                model.FilterByContentType = restrictedContentType;
            }

            if (container != null) {
                var metadata = container.ContentManager.GetItemMetadata(container);
                model.ContainerDisplayName = metadata.DisplayText;
                if (string.IsNullOrEmpty(model.ContainerDisplayName)) {
                    model.ContainerDisplayName = container.ContentType;
                }
            }

            var query = _contentManager.Query<ContainablePart>(VersionOptions.Latest);

            if (!string.IsNullOrEmpty(model.FilterByContentType)) {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.FilterByContentType);
                if (contentTypeDefinition == null)
                    return HttpNotFound();
                query = query.ForType(model.FilterByContentType);
            }
            query = query.Join<CommonPartRecord>().Where(cr => cr.Container.Id == model.ContainerId);

            switch (model.Options.OrderBy) {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending<CommonPartRecord, DateTime?>(cr => cr.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending<CommonPartRecord, DateTime?>(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending<CommonPartRecord, DateTime?>(cr => cr.CreatedUtc);
                    break;
            }

            model.Options.SelectedFilter = model.FilterByContentType;

            if (!hasRestriction) {
                model.Options.FilterOptions = GetContainableTypes()
                    .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                    .ToList().OrderBy(kvp => kvp.Key);
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(query.Count());
            var pageOfContentItems = query.Slice(pager.GetStartIndex(), pager.PageSize).ToList();

            var list = Shape.List();
            list.AddRange(pageOfContentItems.Select(ci => _contentManager.BuildDisplay(ci, "SummaryAdmin")));

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list)
                .Pager(pagerShape)
                .ContainerId(model.ContainerId)
                .Options(model.Options)
                .HasRestriction(hasRestriction)
                .ContainerDisplayName(model.ContainerDisplayName)
                .ContainerContentType(container == null ? null : container.ContentType)
                .ContainerItemContentType(hasRestriction ? restrictedContentType : (model.FilterByContentType ?? ""))
                .OtherLists(_contentManager.Query<ContainerPart>(VersionOptions.Latest).List()
                    .Select(part => part.ContentItem)
                    .Where(item => item != container)
                    .OrderBy(item => item.As<CommonPart>().VersionPublishedUtc));

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
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
                    Services.Notifier.Information(T("One or more items could not be moved to '{0}' because it is restricted to containing items of type '{1}'.", _contentManager.GetItemMetadata(targetContainer).DisplayText, itemContentType));
                    return true; // todo: transactions
                }

                item.As<CommonPart>().Record.Container = targetContainer.ContentItem.Record;
            }
            Services.Notifier.Information(T("Content successfully moved to <a href=\"{0}\">{1}</a>.",
                                            Url.Action("List", new { containerId = targetContainerId }), _contentManager.GetItemMetadata(targetContainer).DisplayText));
            return true;
        }

        private bool BulkRemoveFromList(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.EditContent, item, T("Couldn't remove selected content from the list."))) {
                    return false;
                }
                item.As<CommonPart>().Record.Container = null;
            }
            Services.Notifier.Information(T("Content successfully removed from the list."));
            return true;
        }

        private bool BulkRemove(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.DeleteContent, item, T("Couldn't remove selected content."))) {
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
                    return false;
                }

                _contentManager.Unpublish(item);
            }
            Services.Notifier.Information(T("Content successfully unpublished."));
            return true;
        }

        private bool BulkPublishNow(IEnumerable<int> itemIds) {
            foreach (var item in itemIds.Select(itemId => _contentManager.GetLatest(itemId))) {
                if (!Services.Authorizer.Authorize(Permissions.PublishContent, item, T("Couldn't publish selected content.")))
                    return false;

                _contentManager.Publish(item);
            }
            Services.Notifier.Information(T("Content successfully published."));
            return true;
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


    }
}
