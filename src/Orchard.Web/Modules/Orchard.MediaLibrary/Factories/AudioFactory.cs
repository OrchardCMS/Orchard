using System.IO;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class AudioFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;

        public AudioFactorySelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
            if (!mimeType.StartsWith("audio/")) {
                return null;
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new AudioFactory(_contentManager)
            };

        }
    }

    public class AudioFactory : IMediaFactory {
        private readonly IContentManager _contentManager;

        public const string BaseFolder = "Audio";

        public AudioFactory(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType) {
            var part = _contentManager.New<MediaPart>("Audio");

            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var audioPart = part.As<AudioPart>();
            audioPart.Length = 0;

            return part;
        }
    }
}