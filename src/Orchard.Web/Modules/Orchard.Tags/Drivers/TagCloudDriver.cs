using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Tags.Models;

namespace Orchard.Tags.Drivers {
    [OrchardFeature("Orchard.Tags.TagCloud")]
    public class TagCloudDriver : ContentPartDriver<TagCloudPart> {

        protected override string Prefix {
            get {
                return "tagcloud";
            }
        }

        protected override DriverResult Display(TagCloudPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_TagCloud",
                () => shapeHelper.Parts_TagCloud(
                    TagCounts: part.TagCounts,
                    ContentPart: part,
                    ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(TagCloudPart part, dynamic shapeHelper) {

            return ContentShape("Parts_TagCloud_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts/TagCloud",
                    Model: part,
                    Prefix: Prefix));
        }
        
        protected override DriverResult Editor(TagCloudPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(TagCloudPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Slug", part.Slug);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Buckets", part.Buckets);
        }

        protected override void Importing(TagCloudPart part, ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            part.Slug = context.Attribute(part.PartDefinition.Name, "Slug");
            part.Buckets = Convert.ToInt32(context.Attribute(part.PartDefinition.Name, "Buckets"));
        }
    }
}