using System.Drawing;
using System.IO;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class ImageFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public ImageFactorySelector(IContentManager contentManager, IStorageProvider storageProvider) {
            _contentManager = contentManager;
            _storageProvider = storageProvider;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
            if (!mimeType.StartsWith("image/")) {
                return null;
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new ImageFactory(_contentManager, _storageProvider)
            };

        }
    }

    public class ImageFactory : IMediaFactory {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public const string BaseFolder = "Images";

        public ImageFactory(IContentManager contentManager, IStorageProvider storageProvider) {
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

            var part = _contentManager.New<MediaPart>("Image");

            if (!_storageProvider.FolderExists(BaseFolder)) {
                _storageProvider.CreateFolder(BaseFolder);
            }

            part.Resource = _storageProvider.GetRelativePath(_storageProvider.Combine(BaseFolder, uniquePath));
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var imagePart = part.As<ImagePart>();
            using (var image = Image.FromStream(stream)) {
                imagePart.Width = image.Width;
                imagePart.Height = image.Height;
            }

            return part;
        }
    }
}