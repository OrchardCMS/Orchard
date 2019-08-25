using System;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class VectorImageFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public VectorImageFactorySelector(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }


        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType, string contentType) {
            if (!mimeType.StartsWith("image/svg")) {
                return null;
            }

            if (!String.IsNullOrEmpty(contentType)) {
                var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentDefinition == null || contentDefinition.Parts.All(x => x.PartDefinition.Name != typeof(VectorImagePart).Name)) {
                    return null;
                }
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new VectorImageFactory(_contentManager)
            };

        }
    }

    public class VectorImageFactory : IMediaFactory {
        private readonly IContentManager _contentManager;

        public VectorImageFactory(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType, string contentType) {
            if (String.IsNullOrEmpty(contentType)) {
                contentType = "VectorImage";
            }

            var part = _contentManager.New<MediaPart>(contentType);

            part.LogicalType = "VectorImage";
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var imagePart = part.As<VectorImagePart>();
            if (imagePart == null) {
                return null;
            }

            return part;
        }
    }
}