using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;

namespace Orchard.Core.Containers.Drivers {
    public class CustomPropertiesPartDriver : ContentPartDriver<CustomPropertiesPart> {
        protected override DriverResult Editor(CustomPropertiesPart part, dynamic shapeHelper) {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(CustomPropertiesPart part, IUpdateModel updater, dynamic shapeHelper) {
            return ContentShape(
                "Parts_CustomProperties_Edit",
                () => {
                    if (updater != null)
                        updater.TryUpdateModel(part, "CustomProperties", null, null);

                    return shapeHelper.EditorTemplate(TemplateName: "CustomProperties", Model: part, Prefix: "CustomProperties");
                });
        }
    }

    public class CustomPropertiesPartHandler : ContentHandler {
        public CustomPropertiesPartHandler(IRepository<CustomPropertiesPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}