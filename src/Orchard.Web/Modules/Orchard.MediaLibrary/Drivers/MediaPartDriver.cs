using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
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
            updater.TryUpdateModel(part, Prefix, new[] { "Caption", "AlternateText" }, null);
            return Editor(part, shapeHelper);
        }

        protected override DriverResult Editor(MediaPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Media_Edit", () => shapeHelper.EditorTemplate(TemplateName: "Parts.Media.Edit", Model: part, Prefix: Prefix));
        }

        protected override void Importing(MediaPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "MimeType", mimeType =>
                part.MimeType = mimeType
            );

            context.ImportAttribute(part.PartDefinition.Name, "Caption", caption =>
                part.Caption = caption
            );

            context.ImportAttribute(part.PartDefinition.Name, "AlternateText", alternateText =>
                part.AlternateText = alternateText
            );

            context.ImportAttribute(part.PartDefinition.Name, "FolderPath", folderPath =>
                part.FolderPath = folderPath
            );

            context.ImportAttribute(part.PartDefinition.Name, "FileName", fileName =>
                part.FileName = fileName
            );

            context.ImportAttribute(part.PartDefinition.Name, "LogicalType", logicalType =>
                part.LogicalType = logicalType
            );
        }

        protected override void Exporting(MediaPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("MimeType", part.MimeType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Caption", part.Caption);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AlternateText", part.AlternateText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FolderPath", part.FolderPath);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FileName", part.FileName);
            context.Element(part.PartDefinition.Name).SetAttributeValue("LogicalType", part.LogicalType);
        }

        protected override void Cloning(MediaPart originalPart, MediaPart clonePart, CloneContentContext context) {
            clonePart.Caption = originalPart.Caption;
            clonePart.FileName = originalPart.FileName;
            clonePart.FolderPath = originalPart.FolderPath;
            clonePart.LogicalType = originalPart.LogicalType;
            clonePart.AlternateText = originalPart.AlternateText;
            clonePart.MimeType = originalPart.MimeType;
        }
    }
}