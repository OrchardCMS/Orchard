using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;

namespace Orchard.Core.Containers.Drivers {
    public class ContainerCustomPartDriver : ContentPartDriver<ContainerCustomPart> {
        protected override DriverResult Editor(ContainerCustomPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ContainerCustomPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_ContainerCustom_Edit",
                () => {
                    if (updater != null)
                        updater.TryUpdateModel(part, "ContainerCustom", null, null);

                    return shapeHelper.EditorTemplate(TemplateName: "ContainerCustom", Model: part, Prefix: "ContainerCustom");
                });
        }
    }

    public class ContainerCustomPartHandler : ContentHandler {
        public ContainerCustomPartHandler(IRepository<ContainerCustomPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}