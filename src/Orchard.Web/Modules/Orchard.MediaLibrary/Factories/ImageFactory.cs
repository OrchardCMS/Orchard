using System.Drawing;
using System.IO;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class ImageFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;

        public ImageFactorySelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
            if (!mimeType.StartsWith("image/")) {
                return null;
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new ImageFactory(_contentManager)
            };

        }
    }

    public class ImageFactory : IMediaFactory {
        private readonly IContentManager _contentManager;

        public ImageFactory(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType) {

            var part = _contentManager.New<MediaPart>("Image");

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