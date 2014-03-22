using System;
using System.Linq;
using Orchard.Azure.MediaServices.Models.Assets.EncoderMetadata;
namespace Orchard.Azure.MediaServices.Models.Assets {
    public class VideoAsset : Asset {
        private Metadata _encoderMetadata; // TODO: Should be made thread-safe.

        public string WamsEncoderMetadataXml {
            get { return Storage.Get<string>("WamsEncoderMetadataXml"); }
            set {
                Storage.Set("WamsEncoderMetadataXml", value);
                _encoderMetadata = null; // Clear out cached metadata.
            }
        }

        public string EncodingPreset {
            get { return Storage.Get<string>("EncodingPreset"); }
            set { Storage.Set("EncodingPreset", value); }
        }

        public Metadata EncoderMetadata {
            get {
                if (_encoderMetadata == null) {
                    if (!String.IsNullOrEmpty(WamsEncoderMetadataXml)) {
                        _encoderMetadata = Metadata.Parse(WamsEncoderMetadataXml, WamsPrivateLocatorUrl, WamsPublicLocatorUrl, MimeTypeProvider);
                    }
                }
                return _encoderMetadata;
            }
        }

        protected override string GetMainFileUrl(string locatorUrl) {
            // In the case of a video asset we consider the main file to be the first
            // asset file containing one or more video tracks according to the encoder
            // metadata.
            if (!String.IsNullOrEmpty(locatorUrl) && EncoderMetadata != null) {
                var firstVideoFile = EncoderMetadata.AssetFiles.FirstOrDefault(assetFile => assetFile.VideoTracks.Any());
                if (firstVideoFile != null) {
                    var builder = new UriBuilder(locatorUrl);
                    builder.Path += "/" + firstVideoFile.Name;
                    return builder.Uri.AbsoluteUri;
                }
            }
            return null;
        }
    }
}