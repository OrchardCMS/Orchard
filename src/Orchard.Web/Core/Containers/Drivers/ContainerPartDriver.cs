using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
using Orchard.Core.Containers.Extensions;
using System.Web.Routing;
using Orchard.Settings;
using Orchard.Core.Feeds;
using Orchard.UI.Navigation;

namespace Orchard.Core.Containers.Drivers {
    public class ContainerPartDriver : ContentPartDriver<ContainerPart> {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IFeedManager _feedManager;

        public ContainerPartDriver(
            IContentDefinitionManager contentDefinitionManager, 
            IOrchardServices orchardServices, 
            ISiteService siteService,
            IFeedManager feedManager) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _contentManager = orchardServices.ContentManager;
            _siteService = siteService;
            _feedManager = feedManager;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(ContainerPart part, string displayType, dynamic shapeHelper) {
            if (!part.ItemsShown)
                return null;

        return ContentShape("Parts_Container_Contained",
                            () => {
                                var container = part.ContentItem;

                                IContentQuery<ContentItem> query = _contentManager
                                .Query(VersionOptions.Published)
                                .Join<CommonPartRecord>().Where(cr => cr.Container.Id == container.Id);

                                var descendingOrder = part.OrderByDirection == (int)OrderByDirection.Descending;
                                query = query.OrderBy(part.OrderByProperty, descendingOrder);
                                 var metadata = container.ContentManager.GetItemMetadata(container);
                                 if (metadata!=null)
                                    _feedManager.Register(metadata.DisplayText, "rss", new RouteValueDictionary { { "containerid", container.Id } });

                                var pager = new Pager(_siteService.GetSiteSettings(), part.PagerParameters);
                                pager.PageSize = part.PagerParameters.PageSize != null && part.Paginated
                                                ? pager.PageSize
                                                : part.PageSize;

                                var pagerShape = shapeHelper.Pager(pager).TotalItemCount(query.Count());

                                var startIndex = part.Paginated ? pager.GetStartIndex() : 0;
                                var pageOfItems = query.Slice(startIndex, pager.PageSize).ToList();

                                var listShape = shapeHelper.List();
                                listShape.AddRange(pageOfItems.Select(item => _contentManager.BuildDisplay(item, "Summary")));
                                listShape.Classes.Add("content-items");
                                listShape.Classes.Add("list-items");

                                return shapeHelper.Parts_Container_Contained(
                                    List: listShape,
                                    Pager: part.Paginated ? pagerShape : null
                                );
                            });
        }

        protected override DriverResult Editor(ContainerPart part, dynamic shapeHelper) {
            // if there are no containable items then show a nice little warning
            if (!_contentDefinitionManager.ListTypeDefinitions().Any(typeDefinition => typeDefinition.Parts.Any(partDefinition => partDefinition.PartDefinition.Name == "ContainablePart"))) {
                _orchardServices.Notifier.Warning(T("There are no content types in the system with a Containable part attached. Consider adding a Containable part to some content type, existing or new, in order to relate items to this (Container enabled) item."));
            }

            return Editor(part, (IUpdateModel)null, shapeHelper);
        }

        protected override DriverResult Editor(ContainerPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Container_Edit",
                () => {
                    var model = new ContainerViewModel { Part = part };
                    // todo: is there a non-string comparison way to find ContainableParts?
                    var containables = _contentDefinitionManager.ListTypeDefinitions().Where(td => td.Parts.Any(p => p.PartDefinition.Name == "ContainablePart")).ToList();
                    var listItems = new[] { new SelectListItem { Text = T("(Any)").Text, Value = "" } }
                        .Concat(containables.Select(x => new SelectListItem {
                            Value = Convert.ToString(x.Name),
                            Text = x.DisplayName,
                            Selected = x.Name == model.Part.Record.ItemContentType,
                        }))
                        .ToList();

                    model.AvailableContainables = new SelectList(listItems, "Value", "Text", model.Part.ItemContentType);

                    if (updater != null) {
                        updater.TryUpdateModel(model, "Container", null, null);
                    }
                   
                    return shapeHelper.EditorTemplate(TemplateName: "Container", Model: model, Prefix: "Container");
                });
        }

        protected override void Importing(ContainerPart part, ImportContentContext context) {
            var itemContentType = context.Attribute(part.PartDefinition.Name, "ItemContentType");
            if (itemContentType != null) {
                if (_contentDefinitionManager.GetTypeDefinition(itemContentType) != null) {
                    part.Record.ItemContentType = itemContentType;
                }
            }

            var itemsShown = context.Attribute(part.PartDefinition.Name, "ItemsShown");
            if (itemsShown != null) {
                part.Record.ItemsShown = Convert.ToBoolean(itemsShown);
            }

            var paginated = context.Attribute(part.PartDefinition.Name, "Paginated");
            if (paginated != null) {
                part.Record.Paginated = Convert.ToBoolean(paginated);
            }

            var pageSize = context.Attribute(part.PartDefinition.Name, "PageSize");
            if (pageSize != null) {
                part.Record.PageSize = Convert.ToInt32(pageSize);
            }

            var orderByProperty = context.Attribute(part.PartDefinition.Name, "OrderByProperty");
            if (orderByProperty != null) {
                part.Record.OrderByProperty = orderByProperty;
            }

            var orderByDirection = context.Attribute(part.PartDefinition.Name, "OrderByDirection");
            if (orderByDirection != null) {
                part.Record.OrderByDirection = Convert.ToInt32(orderByDirection);
            }
        }

        protected override void Exporting(ContainerPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("ItemContentType", part.Record.ItemContentType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ItemsShown", part.Record.ItemsShown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Paginated", part.Record.Paginated);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PageSize", part.Record.PageSize);
            context.Element(part.PartDefinition.Name).SetAttributeValue("OrderByProperty", part.Record.OrderByProperty);
            context.Element(part.PartDefinition.Name).SetAttributeValue("OrderByDirection", part.Record.OrderByDirection);
        }
    }
}
