using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard.MediaLibrary.Models;
using Orchard.MediaLibrary.Services;

namespace Orchard.MediaLibrary.Drivers {
    public class MediaPartDriver : ContentPartDriver<MediaPart> {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IStorageProvider _storageProvider;

        protected override string Prefix {
            get { return "MediaPart"; }
        }

        public MediaPartDriver(
            IMediaLibraryService mediaLibraryService,
            IStorageProvider storageProvider,
            IOrchardServices services
            ) {
            _mediaLibraryService = mediaLibraryService;
            _storageProvider = storageProvider;
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

        protected override void Importing(MediaPart part, ImportContentContext context) {
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

            if (context.Files == null) return;
            var path = Path.Combine(part.FolderPath, part.FileName);
            var file = context.Files
                .FirstOrDefault(f => f.Path.StartsWith("\\Media\\") && f.Path.Substring(7) == path);
            if (file == null) return;
            using (var stream = file.GetStream()) {
                var filePath = Path.Combine(part.FolderPath, part.FileName);
                if (_storageProvider.FileExists(filePath)) {
                    _storageProvider.DeleteFile(filePath);
                }
                var publicUrl = _mediaLibraryService.UploadMediaFile(part.FolderPath, part.FileName, stream);
                part._publicUrl.Value = publicUrl;
            }
        }

        protected override void Exporting(MediaPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("MimeType", part.MimeType);
            context.Element(part.PartDefinition.Name).SetAttributeValue("Caption", part.Caption);
            context.Element(part.PartDefinition.Name).SetAttributeValue("AlternateText", part.AlternateText);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FolderPath", part.FolderPath);
            context.Element(part.PartDefinition.Name).SetAttributeValue("FileName", part.FileName);

            if (part.FolderPath != null && part.FileName != null) {
                var path = Path.Combine(part.FolderPath, part. FileName);
                var file = _storageProvider.GetFile(path);
                if (file != null) {
                    context.AddFile(Path.Combine("Media", path), file);
                }
            }
        }
    }
}