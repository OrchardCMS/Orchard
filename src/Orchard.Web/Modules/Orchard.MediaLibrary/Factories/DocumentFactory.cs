using System;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    /// <summary>
    /// The <see cref="DocumentFactorySelector"/> class is a fallback factory which create a Document media 
    /// if no other factory could handle the file.
    /// </summary>
    public class DocumentFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DocumentFactorySelector(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType, string contentType) {
            if (!String.IsNullOrEmpty(contentType)) {
                var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentDefinition == null || contentDefinition.Parts.All(x => x.PartDefinition.Name != typeof(DocumentPart).Name)) {
                    return null;
                }
            }
            
            return new MediaFactorySelectorResult {
                Priority = -10,
                MediaFactory = new DocumentFactory(_contentManager)
            };
        }
    }

    public class DocumentFactory : IMediaFactory {
        private readonly IContentManager _contentManager;

        public DocumentFactory(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType, string contentType) {

            if (String.IsNullOrEmpty(contentType)) {
                contentType = "Document";
            }

            var part = _contentManager.New<MediaPart>(contentType);

            part.LogicalType = "Document";
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var documentPart = part.As<DocumentPart>();

            if (documentPart == null) {
                return null;
            }
            
            documentPart.Length = stream.Length;

            return part;
        }
    }
}