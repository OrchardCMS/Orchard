using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.ViewModels;
using Orchard.Localization;

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
                    var commonPart = part.As<ICommonPart>();

                    var model = new ContainableViewModel();
                    if (commonPart != null && commonPart.Container != null) {
                        model.ContainerId = commonPart.Container.Id;
                    }

                    if (updater != null) {
                        var oldContainerId = model.ContainerId;
                        updater.TryUpdateModel(model, "Containable", null, null);
                        if (oldContainerId != model.ContainerId)
                            if (commonPart != null) {
                                commonPart.Container = _contentManager.Get(model.ContainerId, VersionOptions.Latest);
                            }
                    }

                    // note: string.isnullorempty not being recognized by linq-to-nhibernate hence the inline or
                    var containers = _contentManager.Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest)
                        .Where(ctr => ctr.ItemContentType == null || ctr.ItemContentType == "" || ctr.ItemContentType == part.ContentItem.ContentType).List();

                    var listItems = new[] { new SelectListItem { Text = T("(None)").Text, Value = "0" } }
                        .Concat(containers.Select(x => new SelectListItem {
                            Value = Convert.ToString(x.Id),
                            Text = x.ContentItem.TypeDefinition.DisplayName + ": " + x.As<IRoutableAspect>().Title,
                            Selected = x.Id == model.ContainerId,
                        }))
                        .ToList();

                    model.AvailableContainers = new SelectList(listItems, "Value", "Text", model.ContainerId);

                    return shapeHelper.EditorTemplate(TemplateName: "Containable", Model: model, Prefix: "Containable");
                });
        }
    }
}
