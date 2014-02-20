using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
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
    }
}