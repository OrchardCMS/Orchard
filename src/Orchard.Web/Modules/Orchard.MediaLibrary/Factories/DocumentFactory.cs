using System.IO;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    /// <summary>
    /// The <see cref="DocumentFactorySelector"/> class is a fallback factory which create a Document media 
    /// if no other factory could handle the file.
    /// </summary>
    public class DocumentFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public DocumentFactorySelector(IContentManager contentManager, IStorageProvider storageProvider) {
            _contentManager = contentManager;
            _storageProvider = storageProvider;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
            return new MediaFactorySelectorResult {
                Priority = -10,
                MediaFactory = new DocumentFactory(_contentManager, _storageProvider)
            };
        }
    }

    public class DocumentFactory : IMediaFactory {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public const string BaseFolder = "Documents";

        public DocumentFactory(IContentManager contentManager, IStorageProvider storageProvider) {
            _contentManager = contentManager;
            _storageProvider = storageProvider;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType) {
            var uniquePath = path;
            var index = 1;
            while (_storageProvider.FileExists(_storageProvider.Combine(BaseFolder, uniquePath))) {
                uniquePath = Path.GetFileNameWithoutExtension(path) + "-" + index++ + Path.GetExtension(path);
            }

            _storageProvider.SaveStream(_storageProvider.Combine(BaseFolder, uniquePath), stream);

            var part = _contentManager.New<MediaPart>("Document");

            if (!_storageProvider.FolderExists(BaseFolder)) {
                _storageProvider.CreateFolder(BaseFolder);
            }

            part.Resource = _storageProvider.GetPublicUrl(_storageProvider.Combine(BaseFolder, uniquePath));
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var documentPart = part.As<DocumentPart>();
            documentPart.Length = stream.Length;

            return part;
        }
    }
}