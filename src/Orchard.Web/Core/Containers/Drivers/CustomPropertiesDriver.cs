using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;

namespace Orchard.Core.Containers.Drivers {
    public class CustomPropertiesPartDriver : ContentPartDriver<CustomPropertiesPart> {
        protected override DriverResult Editor(CustomPropertiesPart part, dynamic shapeHelper) {
            return Editor(part, (IUpdateModel)null, shapeHelper);
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

        protected override void Importing(CustomPropertiesPart part, ImportContentContext context) {
            var customOne = context.Attribute(part.PartDefinition.Name, "CustomOne");
            if (customOne != null) {
                part.Record.CustomOne = customOne;
            }

            var customTwo = context.Attribute(part.PartDefinition.Name, "CustomTwo");
            if (customTwo != null) {
                part.Record.CustomTwo = customTwo;
            }

            var customThree = context.Attribute(part.PartDefinition.Name, "CustomThree");
            if (customThree != null) {
                part.Record.CustomThree = customThree;
            }
        }

        protected override void Exporting(CustomPropertiesPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomOne", part.Record.CustomOne);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomTwo", part.Record.CustomTwo);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomThree", part.Record.CustomThree);
        }
    }

    public class CustomPropertiesPartHandler : ContentHandler {
        public CustomPropertiesPartHandler(IRepository<CustomPropertiesPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}