using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Containers.Models;
using Orchard.Data;

namespace Orchard.Core.Containers.Drivers {
    [Obsolete("Use Fields instead.")]
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
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "CustomOne", customOne =>
                part.Record.CustomOne = customOne
            );

            context.ImportAttribute(part.PartDefinition.Name, "CustomTwo", customTwo =>
                part.Record.CustomTwo = customTwo
            );

            context.ImportAttribute(part.PartDefinition.Name, "CustomThree", customThree =>
                part.Record.CustomThree = customThree
            );
        }

        protected override void Exporting(CustomPropertiesPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomOne", part.Record.CustomOne);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomTwo", part.Record.CustomTwo);
            context.Element(part.PartDefinition.Name).SetAttributeValue("CustomThree", part.Record.CustomThree);
        }
    }

    [Obsolete("Use Fields instead.")]
    public class CustomPropertiesPartHandler : ContentHandler {
        public CustomPropertiesPartHandler(IRepository<CustomPropertiesPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}