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

        protected override void Importing(MediaPart part, ContentManagement.Handlers.ImportContentContext context) {
            var mimeType = context.Attribute(part.PartDefinition.Name, "MimeType");
            if (mimeType != null) {
                part.MimeType = mimeType;
            }

            var caption = context.Attribute(part.PartDefinition.Name, "Caption");
            if (caption != null) {
                part.Caption = caption;
            }

            var alternateText = context.Attribute(part.PartDefinition.Name, "AlternateText");
            if (alternateText != null) {
                part.AlternateText = alternateText;
            }

            var folderPath = context.Attribute(part.PartDefinition.Name, "FolderPath");
            if (folderPath != null) {
                part.FolderPath = folderPath;
            }

            var fileName = context.Attribute(part.PartDefinition.Name, "FileName");
            if (fileName != null) {
                part.FileName = fileName;
            }
        }

        protected override void Exporting(MediaPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("MimeType", part.MimeType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Caption", part.Caption);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AlternateText", part.AlternateText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FolderPath", part.FolderPath);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FileName", part.FileName);
        }
    }
}