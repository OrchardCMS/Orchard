using System;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Services;
using Orchard.Core.Containers.Settings;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Core.Containers.Drivers {
    public class ContainablePartDriver : ContentPartDriver<ContainablePart> {
        private readonly IContentManager _contentManager;
        private readonly IContainerService _containerService;

        public ContainablePartDriver(IContentManager contentManager, IContainerService containerService) {
            _contentManager = contentManager;
            _containerService = containerService;
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
                    var settings = part.TypePartDefinition.Settings.GetModel<ContainableTypePartSettings>();
                    var commonPart = part.As<CommonPart>();
                    var model = new ContainableViewModel {
                        ShowContainerPicker = settings.ShowContainerPicker,
                        ShowPositionEditor = settings.ShowPositionEditor,
                        Position = part.Position
                    };

                    if (commonPart != null && commonPart.Container != null) {
                        model.ContainerId = commonPart.Container.Id;
                    }

                    if (part.Id == 0 && commonPart != null && commonPart.Container != null) {
                        part.Position = _containerService.GetFirstPosition(commonPart.Container.Id) + 1;
                    }

                    if (updater != null) {
                        var oldContainerId = model.ContainerId;
                        updater.TryUpdateModel(model, "Containable", null, new[] { "ShowContainerPicker", "ShowPositionEditor" });
                        if (oldContainerId != model.ContainerId && settings.ShowContainerPicker) {
                            if (commonPart != null) {
                                var containerItem = _contentManager.Get(model.ContainerId, VersionOptions.Latest);
                                commonPart.Container = containerItem;
                            }
                        }
                        part.Position = model.Position;
                    }

                    if (settings.ShowContainerPicker) {
                        var containers = _contentManager
                            .Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest)
                            .List()
                            .Where(container => container.ItemContentTypes.Any(type => type.Name == part.TypeDefinition.Name));

                        var listItems = new[] { new SelectListItem { Text = T("(None)").Text, Value = "0" } }
                            .Concat(containers.Select(x => new SelectListItem {
                                Value = Convert.ToString(x.Id),
                                Text = x.ContentItem.TypeDefinition.DisplayName + ": " + _contentManager.GetItemMetadata(x.ContentItem).DisplayText,
                                Selected = x.Id == model.ContainerId,
                            }))
                            .ToList();

                        model.AvailableContainers = new SelectList(listItems, "Value", "Text", model.ContainerId);
                    }

                    model.Position = part.Position;

                    return shapeHelper.EditorTemplate(TemplateName: "Containable", Model: model, Prefix: "Containable");
                });
        }

        protected override void Importing(ContainablePart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "Position", s => part.Position = XmlConvert.ToInt32(s));
        }

        protected override void Exporting(ContainablePart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Position", part.Position);
        }
    }
}
