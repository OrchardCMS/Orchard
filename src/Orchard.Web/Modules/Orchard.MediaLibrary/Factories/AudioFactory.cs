using System;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class AudioFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AudioFactorySelector(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType, string contentType) {
            if (!mimeType.StartsWith("audio/")) {
                return null;
            }

            if (!String.IsNullOrEmpty(contentType)) {
                var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentDefinition == null || contentDefinition.Parts.All(x => x.PartDefinition.Name != typeof(AudioPart).Name)) {
                    return null;
                }
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

        public MediaPart CreateMedia(Stream stream, string path, string mimeType, string contentType) {
            if (String.IsNullOrEmpty(contentType)) {
                contentType = "Audio";
            }

            var part = _contentManager.New<MediaPart>(contentType);

            part.LogicalType = "Audio";
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var audioPart = part.As<AudioPart>();
            
            if (audioPart == null) {
                return null;
            }

            audioPart.Length = 0;

            return part;
        }
    }
}