using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.MediaLibrary.Models;
using Image = System.Drawing.Image;

namespace Orchard.MediaLibrary.Factories {

    public class ImageFactorySelector : IMediaFactorySelector {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ImageFactorySelector(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }


        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string mimeType, string contentType) {
            if (!mimeType.StartsWith("image/")) {
                return null;
            }
            if (!ImageCodecInfo.GetImageDecoders().Select(d => d.MimeType).Contains(mimeType)) {
                return null;
            }

            if (!String.IsNullOrEmpty(contentType)) {
                var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentDefinition == null || contentDefinition.Parts.All(x => x.PartDefinition.Name != typeof(ImagePart).Name)) {
                    return null;
                }
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

        public MediaPart CreateMedia(Stream stream, string path, string mimeType, string contentType) {
            if (String.IsNullOrEmpty(contentType)) {
                contentType = "Image";
            }

            var part = _contentManager.New<MediaPart>(contentType);

            part.LogicalType = "Image";
            part.MimeType = mimeType;
            part.Title = Path.GetFileNameWithoutExtension(path);

            var imagePart = part.As<ImagePart>();
            if (imagePart == null) {
                return null;
            }

            try {
                using (var image = Image.FromStream(stream)) {
                    imagePart.Width = image.Width;
                    imagePart.Height = image.Height;
                }
            }
            catch (ArgumentException) {
                // Still trying to get .ico dimensions when it's blocked in System.Drawing, see: https://orchard.codeplex.com/workitem/20644

                if (mimeType != "image/x-icon" && mimeType != "image/vnd.microsoft.icon") {
                    throw;
                }

                TryFillDimensionsForIco(stream, imagePart);
            }

            return part;
        }

        private void TryFillDimensionsForIco(Stream stream, ImagePart imagePart) {
            stream.Position = 0;
            using (var binaryReader = new BinaryReader(stream)) {
                // Reading out the necessary bytes that indicate the image dimensions. For the file format see:
                // http://en.wikipedia.org/wiki/ICO_%28file_format%29
                // Reading out leading bytes containing unneded information.
                binaryReader.ReadBytes(6);
                // Reading out dimensions. If there are multiple icons bundled in the same file then this is the first image.
                imagePart.Width = binaryReader.ReadByte();
                imagePart.Height = binaryReader.ReadByte();
            }
        }
    }
}