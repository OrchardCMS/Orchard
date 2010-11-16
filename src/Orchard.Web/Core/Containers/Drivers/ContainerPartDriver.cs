using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Core.Containers.Models;
using Orchard.Core.Containers.Settings;
using Orchard.Data;

namespace Orchard.Core.Containers.Drivers {
    public class ContainerPartDriver : ContentPartDriver<ContainerPart> {
        protected override DriverResult Editor(ContainerPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }
        protected override DriverResult Editor(ContainerPart part, ContentManagement.IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_Container_Edit",
                () => {
                    if (updater != null)
                        updater.TryUpdateModel(part, "Container", null, null);

                    return shapeHelper.EditorTemplate(TemplateName: "Container", Model: part, Prefix: "Container");
                });
        }
    }

    public class ContainerPartHandler : ContentHandler {
        public ContainerPartHandler(IRepository<ContainerPartRecord> repository, IOrchardServices orchardServices) {
            Filters.Add(StorageFilter.For(repository));
            OnInitializing<ContainerPart>((context, part) => {
                part.Record.PageSize = part.Settings.GetModel<ContainerTypePartSettings>().PageSizeDefault
                    ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().PageSizeDefault;
                part.Record.Paginated = part.Settings.GetModel<ContainerTypePartSettings>().PaginatedDefault
                    ?? part.PartDefinition.Settings.GetModel<ContainerPartSettings>().PaginatedDefault;

                //hard-coded defaults for ordering
                part.Record.OrderByProperty = part.Is<CommonPart>() ? "CommonPart.PublishedUtc" : "";
                part.Record.OrderByDirection = (int)OrderByDirection.Descending;
            });
        }
    }
}
