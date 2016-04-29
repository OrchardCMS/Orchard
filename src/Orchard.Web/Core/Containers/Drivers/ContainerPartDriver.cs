using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;
using Orchard.UI.Notify;
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
        private readonly IContainerService _containerService;

        public ContainerPartDriver(
            IContentDefinitionManager contentDefinitionManager,
            IOrchardServices orchardServices,
            ISiteService siteService,
            IFeedManager feedManager, IContainerService containerService) {
            _contentDefinitionManager = contentDefinitionManager;
            _orchardServices = orchardServices;
            _contentManager = orchardServices.ContentManager;
            _siteService = siteService;
            _feedManager = feedManager;
            _containerService = containerService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(ContainerPart part, string displayType, dynamic shapeHelper) {
            if (!part.ItemsShown)
                return null;

            return ContentShape("Parts_Container_Contained", () => {
                var container = part.ContentItem;
                var query = _contentManager
                .Query(VersionOptions.Published)
                .Join<CommonPartRecord>().Where(x => x.Container.Id == container.Id)
                .Join<ContainablePartRecord>().OrderByDescending(x => x.Position);

                var metadata = container.ContentManager.GetItemMetadata(container);
                if (metadata != null) {
                    _feedManager.Register(metadata.DisplayText, "rss", new RouteValueDictionary {{"containerid", container.Id}});
                }

                // Retrieving pager parameters.
                var queryString = _orchardServices.WorkContext.HttpContext.Request.QueryString;

                var page = 0;
                // Don't try to page if not necessary.
                if (part.Paginated && queryString["page"] != null) {
                    Int32.TryParse(queryString["page"], out page);
                }

                var pageSize = part.PageSize;
                // If the container is paginated and pageSize is provided in the query string then retrieve it.
                if (part.Paginated && queryString["pageSize"] != null) {
                    Int32.TryParse(queryString["pageSize"], out pageSize);
                }

                var pager = new Pager(_siteService.GetSiteSettings(), page, pageSize);

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
            if (!_contentDefinitionManager.ListTypeDefinitions().Any(typeDefinition => typeDefinition.Parts.Any(partDefinition => partDefinition.PartDefinition.Name == "ContainablePart"))) {
                _orchardServices.Notifier.Warning(T("There are no content types in the system with a Containable part attached. Consider adding a Containable part to some content type, existing or new, in order to relate items to this (Container enabled) item."));
            }
            return Editor(part, (IUpdateModel)null, shapeHelper);
        }

        protected override DriverResult Editor(ContainerPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape("Parts_Container_Edit", () => {
                if (!part.ContainerSettings.DisplayContainerEditor) {
                    return null;
                }

                var containables = !part.ContainerSettings.RestrictItemContentTypes ? _containerService.GetContainableTypes().ToList() : new List<ContentTypeDefinition>(0);
                var model = new ContainerViewModel {
                    AdminMenuPosition = part.AdminMenuPosition,
                    AdminMenuText = part.AdminMenuText,
                    AdminMenuImageSet = part.AdminMenuImageSet,
                    ItemsShown = part.ItemsShown,
                    PageSize = part.PageSize,
                    Paginated = part.Paginated,
                    SelectedItemContentTypes = part.ItemContentTypes.Select(x => x.Name).ToList(),
                    ShowOnAdminMenu = part.ShowOnAdminMenu,
                    AvailableItemContentTypes = containables,
                    RestrictItemContentTypes = part.ContainerSettings.RestrictItemContentTypes,
                    EnablePositioning = part.Record.EnablePositioning,
                    OverrideEnablePositioning = part.ContainerSettings.EnablePositioning == null
                };

                if (updater != null) {
                    if (updater.TryUpdateModel(model, "Container", null, new[] { "OverrideEnablePositioning" })) {
                        part.AdminMenuPosition = model.AdminMenuPosition;
                        part.AdminMenuText = model.AdminMenuText;
                        part.AdminMenuImageSet = model.AdminMenuImageSet;
                        part.ItemsShown = model.ItemsShown;
                        part.PageSize = model.PageSize;
                        part.Paginated = model.Paginated;
                        part.ShowOnAdminMenu = model.ShowOnAdminMenu;

                        if (!part.ContainerSettings.RestrictItemContentTypes) {
                            part.ItemContentTypes = _contentDefinitionManager.ListTypeDefinitions().Where(x => model.SelectedItemContentTypes.Contains(x.Name));
                        }

                        if (model.OverrideEnablePositioning) {
                            part.Record.EnablePositioning = model.EnablePositioning;
                        }
                    }
                }

                return shapeHelper.EditorTemplate(TemplateName: "Container", Model: model, Prefix: "Container");
            });
        }

        protected override void Importing(ContainerPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "ItemContentTypes", itemContentType => {
                if (_contentDefinitionManager.GetTypeDefinition(itemContentType) != null) {
                    part.Record.ItemContentTypes = itemContentType;
                }
            });

            context.ImportAttribute(part.PartDefinition.Name, "ItemsShown", s => part.ItemsShown = XmlConvert.ToBoolean(s));
            context.ImportAttribute(part.PartDefinition.Name, "Paginated", s => part.Paginated = XmlConvert.ToBoolean(s));
            context.ImportAttribute(part.PartDefinition.Name, "PageSize", s => part.PageSize = XmlConvert.ToInt32(s));
            context.ImportAttribute(part.PartDefinition.Name, "ShowOnAdminMenu", s => part.ShowOnAdminMenu = XmlConvert.ToBoolean(s));
            context.ImportAttribute(part.PartDefinition.Name, "AdminMenuText", s => part.AdminMenuText = s);
            context.ImportAttribute(part.PartDefinition.Name, "AdminMenuPosition", s => part.AdminMenuPosition = s);
            context.ImportAttribute(part.PartDefinition.Name, "AdminMenuImageSet", s => part.AdminMenuImageSet = s);
            context.ImportAttribute(part.PartDefinition.Name, "ItemCount", s => part.ItemCount = XmlConvert.ToInt32(s));
        }

        protected override void Exporting(ContainerPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("ItemContentTypes", part.Record.ItemContentTypes);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ItemsShown", part.ItemsShown);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Paginated", part.Paginated);
            context.Element(part.PartDefinition.Name).SetAttributeValue("PageSize", part.PageSize);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ShowOnAdminMenu", part.ShowOnAdminMenu);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuText", part.AdminMenuText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuPosition", part.AdminMenuPosition);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AdminMenuImageSet", part.AdminMenuImageSet);
            context.Element(part.PartDefinition.Name).SetAttributeValue("ItemCount", part.ItemCount);
        }
    }
}
