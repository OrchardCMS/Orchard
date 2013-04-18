using Orchard.ContentManagement.Drivers;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class MediaPartDriver : ContentPartDriver<MediaPart> {
        protected override string Prefix {
            get { return "MediaPart"; }
        }

        protected override DriverResult Display(MediaPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Media_SummaryAdmin", () => shapeHelper.Parts_Media_SummaryAdmin()),
                ContentShape("Parts_Media_Actions", () => shapeHelper.Parts_Media_Actions())
            );

        }

        protected override DriverResult Editor(MediaPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Media_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts.Media.Edit", Model: part, Prefix: Prefix));
        }
    }
}