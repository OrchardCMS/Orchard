using System.IO;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class VideoFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;

        public VideoFactorySelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
            if (!mimeType.StartsWith("video/")) {
                return null;
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new VideoFactory(_contentManager)
            };

        }
    }

    public class VideoFactory : IMediaFactory {
        private readonly IContentManager _contentManager;

        public VideoFactory(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType) {
            var part = _contentManager.New<MediaPart>("Video");

            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var videoPart = part.As<VideoPart>();
            videoPart.Length = 0;
            
            return part;
        }
    }
}