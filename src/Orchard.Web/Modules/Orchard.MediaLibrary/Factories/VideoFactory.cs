using System;
using System.IO;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class VideoFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public VideoFactorySelector(IContentManager contentManager, IStorageProvider storageProvider) {
            _contentManager = contentManager;
            _storageProvider = storageProvider;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType) {
            if (!mimeType.StartsWith("video/")) {
                return null;
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new VideoFactory(_contentManager, _storageProvider)
            };

        }
    }

    public class VideoFactory : IMediaFactory {
        private readonly IContentManager _contentManager;
        private readonly IStorageProvider _storageProvider;

        public const string BaseFolder = "Videos";

        public VideoFactory(IContentManager contentManager, IStorageProvider storageProvider) {
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

            var part = _contentManager.New<MediaPart>("Video");

            if (!_storageProvider.FolderExists(BaseFolder)) {
                _storageProvider.CreateFolder(BaseFolder);
            }

            part.Resource = _storageProvider.GetPublicUrl(_storageProvider.Combine(BaseFolder, uniquePath));
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var videoPart = part.As<VideoPart>();
            videoPart.Length = 0;
            
            return part;
        }
    }
}