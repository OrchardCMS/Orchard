using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.ViewModels;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Core.Containers.Drivers {
    public class ContainablePartDriver : ContentPartDriver<ContainablePart> {
        private readonly IContentManager _contentManager;
        private readonly IRoutableService _routableService;
        private readonly IOrchardServices _services;

        public ContainablePartDriver(IContentManager contentManager, IRoutableService routableService, IOrchardServices services) {
            _contentManager = contentManager;
            _routableService = routableService;
            _services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(ContainablePart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ContainablePart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Containable_Edit",
                () => {
                    var commonPart = part.As<ICommonPart>();

                    var model = new ContainableViewModel();
                    if (commonPart.Container != null) {
                        model.ContainerId = commonPart.Container.Id;
                    }

                    if (updater != null) {
                        var oldContainerId = model.ContainerId;
                        updater.TryUpdateModel(model, "Containable", null, null);
                        if (oldContainerId != model.ContainerId) {
                            commonPart.Container = _contentManager.Get(model.ContainerId, VersionOptions.Latest);
                            // reprocess  slug
                            var routable = part.As<IRoutableAspect>();
                            _routableService.ProcessSlug(part.As<IRoutableAspect>());
                            if (!_routableService.ProcessSlug(routable)) {
                                var existingConflict = _services.Notifier.List().FirstOrDefault(n => n.Message.TextHint == "Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"");
                                if (existingConflict != null)
                                    existingConflict.Message = T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                                                                 routable.Slug, routable.GetEffectiveSlug(), routable.ContentItem.ContentType);
                                else
                                    _services.Notifier.Warning(T("Slugs in conflict. \"{0}\" is already set for a previously created {2} so now it has the slug \"{1}\"",
                                                                 routable.Slug, routable.GetEffectiveSlug(), routable.ContentItem.ContentType));
                            }
                        }
                    }

                    var containers = _contentManager.Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest).List();
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
