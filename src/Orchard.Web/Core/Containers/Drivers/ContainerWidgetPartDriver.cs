using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Data;
using Orchard.Localization;

namespace Orchard.Core.Containers.Drivers {
    public class ContainerWidgetPartDriver : ContentPartDriver<ContainerWidgetPart> {
        private readonly IContentManager _contentManager;

        public ContainerWidgetPartDriver(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(ContainerWidgetPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ContainerWidgetPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_ContainerWidget_Edit",
                () => {
                    if (updater != null) 
                        updater.TryUpdateModel(part, "ContainerSelector", null, null);

                    var containers = _contentManager.Query<ContainerPart, ContainerPartRecord>(VersionOptions.Latest).List();

                    var listItems = containers.Count() < 1
                        ? new[] {new SelectListItem {Text = T("(None - create container enabled items first)").Text, Value = "0"}}
                        : containers.Select(x => new SelectListItem {
                                Value = Convert.ToString(x.Id),
                                Text = x.ContentItem.TypeDefinition.DisplayName + ": " + x.As<IRoutableAspect>().Title,
                                Selected = x.Id == part.Record.ContainerId,
                            });

                    part.AvailableContainers = new SelectList(listItems, "Value", "Text", part.Record.ContainerId);

                    return shapeHelper.EditorTemplate(TemplateName: "ContainerWidget", Model: part, Prefix: "ContainerWidget");
                });
        }
    }

    public class ContainerWidgetPartHandler : ContentHandler {
        public ContainerWidgetPartHandler(IRepository<ContainerWidgetPartRecord> repository, IOrchardServices orchardServices) {
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<ContainerWidgetPart>((context, part) => {
                part.Record.ContainerId = 0;
                part.Record.PageSize = 5;
                part.Record.OrderByProperty = part.Is<CommonPart>() ? "CommonPart.PublishedUtc" : "";
                part.Record.OrderByDirection = (int)OrderByDirection.Descending;
            });
        }
    }
}
