using System.IO;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    /// <summary>
    /// The <see cref="DocumentFactorySelector"/> class is a fallback factory which create a Document media 
    /// if no other factory could handle the file.
    /// </summary>
    public class DocumentFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;

        public DocumentFactorySelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
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

        public MediaPart CreateMedia(Stream stream, string path, string mimeType) {

            var part = _contentManager.New<MediaPart>("Document");

            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var documentPart = part.As<DocumentPart>();
            documentPart.Length = stream.Length;

            return part;
        }
    }
}