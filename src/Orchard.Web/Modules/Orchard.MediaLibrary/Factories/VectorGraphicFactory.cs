using System;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Models;

namespace Orchard.MediaLibrary.Factories {

    public class VectorGraphicFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public VectorGraphicFactorySelector(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }


        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType, string contentType) {
            if (!mimeType.StartsWith("image/svg")) {
                return null;
            }
            
            if (!String.IsNullOrEmpty(contentType)) {
                var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentDefinition == null || contentDefinition.Parts.All(x => x.PartDefinition.Name != typeof(VectorGraphicPart).Name)) {
                    return null;
                }
            }

            return new MediaFactorySelectorResult {
                Priority = -5,
                MediaFactory = new VectorGraphicFactory(_contentManager)
            };

        }
    }

    public class VectorGraphicFactory : IMediaFactory {
        private readonly IContentManager _contentManager;

        public VectorGraphicFactory(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public MediaPart CreateMedia(Stream stream, string path, string mimeType, string contentType) {
            if (String.IsNullOrEmpty(contentType)) {
                contentType = "VectorGraphic";
            }

            var part = _contentManager.New<MediaPart>(contentType);

            part.LogicalType = "VectorGraphic";
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var imagePart = part.As<VectorGraphicPart>();
            if (imagePart == null) {
                return null;
            }

            return part;
        }
    }
}