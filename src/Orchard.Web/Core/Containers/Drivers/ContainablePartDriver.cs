using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Containers.Drivers {
    public class ContainablePartDriver : ContentPartDriver<ContainablePart> {
        private readonly IContentManager _contentManager;

        public ContainablePartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(ContainablePart part, dynamic shapeHelper) {
            return Editor(part, (IUpdateModel)null, shapeHelper);
        }

        protected override DriverResult Editor(ContainablePart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Containable_Edit",
                () => {
                    var commonPart = part.As<CommonPart>();

                    var model = new ContainableViewModel();
                    if (commonPart != null && commonPart.Container != null) {
                        model.ContainerId = commonPart.Container.Id;
                    }

                    if (updater != null) {
                        var oldContainerId = model.ContainerId;
                        updater.TryUpdateModel(model, "Containable", null, null);
                        if (oldContainerId != model.ContainerId) {
                            if (commonPart != null) {
                                var containerItem = _contentManager.Get(model.ContainerId, VersionOptions.Latest);
                                commonPart.Container = containerItem;
                            }
                        }
                        part.Weight = model.Weight;
                    }

                    // note: string.isnullorempty not being recognized by linq-to-nhibernate hence the inline or
                    var containers = _contentManager.Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest)
                        .Where(ctr => ctr.ItemContentType == null || ctr.ItemContentType == "" || ctr.ItemContentType == part.ContentItem.ContentType).List();

                    var listItems = new[] { new SelectListItem { Text = T("(None)").Text, Value = "0" } }
                        .Concat(containers.Select(x => new SelectListItem {
                            Value = Convert.ToString(x.Id),
                            Text = x.ContentItem.TypeDefinition.DisplayName + ": " + _contentManager.GetItemMetadata(x.ContentItem).DisplayText,
                            Selected = x.Id == model.ContainerId,
                        }))
                        .ToList();

                    model.AvailableContainers = new SelectList(listItems, "Value", "Text", model.ContainerId);
                    model.Weight = part.Weight;

                    return shapeHelper.EditorTemplate(TemplateName: "Containable", Model: model, Prefix: "Containable");
                });
        }

        protected override void Importing(ContainablePart part, ImportContentContext context) {
            var weight = context.Attribute(part.PartDefinition.Name, "Weight");
            if (weight != null) {
                part.Weight = Convert.ToInt32(weight);
            }
        }

        protected override void Exporting(ContainablePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Weight", part.Weight);
        }
    }
}
