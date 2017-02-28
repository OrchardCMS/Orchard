using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentTypes.Events;
using Orchard.MediaLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.MediaLibrary.Handlers {
    /// <summary>
    /// Automatically adds MediaPart to the content type if a content part is attached that needs it.
    /// </summary>
    public class MediaItemHandler : IContentDefinitionEventHandler {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public MediaItemHandler(IContentDefinitionManager contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public void ContentFieldAttached(ContentFieldAttachedContext context) {
        }

        public void ContentFieldDetached(ContentFieldDetachedContext context) {
        }

        public void ContentPartAttached(ContentPartAttachedContext context) {
            AlterMediaItem(_contentDefinitionManager.GetTypeDefinition(context.ContentTypeName));
        }

        public void ContentPartCreated(ContentPartCreatedContext context) {
        }

        public void ContentPartDetached(ContentPartDetachedContext context) {
        }

        public void ContentPartImported(ContentPartImportedContext context) {
        }

        public void ContentPartImporting(ContentPartImportingContext context) {
        }

        public void ContentPartRemoved(ContentPartRemovedContext context) {
        }

        public void ContentTypeCreated(ContentTypeCreatedContext context) {
            AlterMediaItem(context.ContentTypeDefinition);
        }

        public void ContentTypeImported(ContentTypeImportedContext context) {
            AlterMediaItem(context.ContentTypeDefinition);
        }

        public void ContentTypeImporting(ContentTypeImportingContext context) {
        }

        public void ContentTypeRemoved(ContentTypeRemovedContext context) {
        }

        private void AlterMediaItem(ContentTypeDefinition contentTypeDefinition) {
            var partNames = new string[]{
                    typeof(ImagePart).Name,
                    typeof(VectorImagePart).Name,
                    typeof(VideoPart).Name,
                    typeof(AudioPart).Name,
                    typeof(DocumentPart).Name,
                    typeof(OEmbedPart).Name };
            if (contentTypeDefinition != null &&
                contentTypeDefinition.Parts.Any(contentTypePartDefinition =>
                    partNames.Contains(contentTypePartDefinition.PartDefinition.Name))) {
                _contentDefinitionManager.AlterTypeDefinition(contentTypeDefinition.Name,
                    cfg => cfg
                        .WithPart(typeof(MediaPart).Name)
                        .WithSetting("Stereotype", "Media"));
            }
        }
    }
}