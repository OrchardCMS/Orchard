using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Extensions;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;

namespace Orchard.Core.Containers.Drivers {
    public class ContainerWidgetPartDriver : ContentPartDriver<ContainerWidgetPart> {
        private readonly IContentManager _contentManager;

        public ContainerWidgetPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Display(ContainerWidgetPart part, string displayType, dynamic shapeHelper) {
            return ContentShape(
                "Parts_ContainerWidget",
                () => {
                    var container = part.Record.ContainerId != 0 ? _contentManager.Get(part.Record.ContainerId) : default(ContentItem);

                    if (container == null)
                        return null;

                    IContentQuery<ContentItem> query = _contentManager
                        .Query(VersionOptions.Published)
                        .Join<CommonPartRecord>().Where(cr => cr.Container.Id == container.Id);

                    if (part.Record.ApplyFilter)
                        query = query.Where(part.Record.FilterByProperty, part.Record.FilterByOperator, part.Record.FilterByValue);

                    var pageOfItems = query.Slice(0, part.Record.PageSize).ToList();

                    var list = shapeHelper.List();
                    list.AddRange(pageOfItems.Select(item => _contentManager.BuildDisplay(item, "Summary")));

                    return shapeHelper.Parts_ContainerWidget(ContentItems: list);
                });
        }

        protected override DriverResult Editor(ContainerWidgetPart part, dynamic shapeHelper) {
            return Editor(part, (IUpdateModel)null, shapeHelper);
        }

        protected override DriverResult Editor(ContainerWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_ContainerWidget_Edit",
                () => {
                    var model = new ContainerWidgetViewModel {Part = part};
                    var containers = _contentManager.Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest).List().ToArray();

                    if (updater != null) {
                        updater.TryUpdateModel(model, "ContainerWidget", null, null);

                        if (model.Part.Record.ContainerId == 0)
                            updater.AddModelError("Part.Record.ContainerId", containers.Any()
                                ? T("Please select a container to show items from.")
                                : T("Please create a container so you can select it to show items from."));
                    }

                    var listItems = !containers.Any()
                        ? new[] {new SelectListItem {Text = T("(None - create container enabled items first)").Text, Value = "0"}}
                        : containers.Select(x => new SelectListItem {
                                Value = Convert.ToString(x.Id),
                                Text = x.ContentItem.TypeDefinition.DisplayName + ": " + _contentManager.GetItemMetadata(x.ContentItem).DisplayText,
                                Selected = x.Id == model.Part.Record.ContainerId,
                            });

                    model.AvailableContainers = new SelectList(listItems, "Value", "Text", model.Part.Record.ContainerId);

                    return shapeHelper.EditorTemplate(TemplateName: "ContainerWidget", Model: model, Prefix: "ContainerWidget");
                });
        }

        protected override void Importing(ContainerWidgetPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Container", containerIdentity => {
                var container = context.GetItemFromSession(containerIdentity);
                if (container != null) {
                    part.Record.ContainerId = container.Id;
                }
            });

            context.ImportAttribute(part.PartDefinition.Name, "PageSize", pageSize =>
                part.Record.PageSize = Convert.ToInt32(pageSize)
            );

            context.ImportAttribute(part.PartDefinition.Name, "FilterByValue", filterByValue =>
                part.Record.FilterByValue = filterByValue
            );
        }

        protected override void Exporting(ContainerWidgetPart part, ExportContentContext context) {
            var container = _contentManager.Get(part.Record.ContainerId);
            if (container != null) {
                var containerIdentity = _contentManager.GetItemMetadata(container).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("Container", containerIdentity.ToString());
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("PageSize", part.Record.PageSize);
        }
    }
}
