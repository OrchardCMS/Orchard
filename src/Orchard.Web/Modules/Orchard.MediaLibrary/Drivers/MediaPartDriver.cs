using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Drivers {
    public class MediaPartDriver : ContentPartDriver<MediaPart> {

        protected override string Prefix {
            get { return "MediaPart"; }
        }

        public MediaPartDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public IOrchardServices Services { get; set; }

        protected override DriverResult Display(MediaPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Media_SummaryAdmin", () => shapeHelper.Parts_Media_SummaryAdmin()),
                ContentShape("Parts_Media_Actions", () => shapeHelper.Parts_Media_Actions())
            );

        }

        protected override DriverResult Editor(MediaPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, new[] {"Caption", "AlternateText"}, null);
            return Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(MediaPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Media_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts.Media.Edit", Model: part, Prefix: Prefix));
        }
    }
}