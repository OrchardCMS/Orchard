﻿using System;
using System.Linq;
using System.Threading.Tasks;
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
            return ContentShapeAsync(
                "Parts_ContainerWidget",
                async () => {
                    var container = _contentManager.Get(part.Record.ContainerId);

                    IContentQuery<ContentItem> query = _contentManager
                        .Query(VersionOptions.Published)
                        .Join<CommonPartRecord>().Where(cr => cr.Container.Id == container.Id);

                    if (part.Record.ApplyFilter)
                        query = query.Where(part.Record.FilterByProperty, part.Record.FilterByOperator, part.Record.FilterByValue);

                    var pageOfItems = query.Slice(0, part.Record.PageSize).ToList();

                    var list = shapeHelper.List();

                    var tasks = pageOfItems.Select(async item => await _contentManager.BuildDisplayAsync(item, "Summary")).ToArray();

                    await Task.WhenAll(tasks);

                    list.AddRange(tasks.Select(t => t.Result));

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

                    if (updater != null) {
                        updater.TryUpdateModel(model, "ContainerWidget", null, null);
                    }

                    var containers = _contentManager.Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest).List();
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
            var containerIdentity = context.Attribute(part.PartDefinition.Name, "Container");
            if (containerIdentity != null) {
                var container = context.GetItemFromSession(containerIdentity);
                if (container != null) {
                    part.Record.ContainerId = container.Id;
                }
            }

            var pageSize = context.Attribute(part.PartDefinition.Name, "PageSize");
            if (pageSize != null) {
                part.Record.PageSize = Convert.ToInt32(pageSize);
            }

            var filterByValue = context.Attribute(part.PartDefinition.Name, "FilterByValue");
            if (filterByValue != null) {
                part.Record.FilterByValue = filterByValue;
            }
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
