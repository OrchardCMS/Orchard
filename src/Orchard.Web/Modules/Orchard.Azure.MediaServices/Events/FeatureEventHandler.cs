using Orchard.Azure.MediaServices.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Azure.MediaServices.Events {
    public class FeatureEventHandler : IFeatureEventHandler {

        private readonly IOrchardServices _orchardServices;

        public FeatureEventHandler(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public void Installing(Feature feature) {
            
        }

        public void Installed(Feature feature) {
            if (feature.Descriptor.Id != "Orchard.Azure.MediaServices")
                return;
            
            var settings = _orchardServices.WorkContext.CurrentSite.As<CloudMediaSettingsPart>();
            settings.AllowedVideoFilenameExtensions = "asf;avi;m2ts;m2v;mp4;mpeg;mpg;mts;ts;wmv;3gp;3g2;3gp2;mod;dv;vob;ismv;m4a".Split(';');
            settings.WamsEncodingPresets = new[] {
                "VC1 Broadband 1080p",
                "VC1 Broadband 720p",
                "VC1 Broadband SD 16x9",
                "VC1 Broadband SD 4x3",
                "VC1 Smooth Streaming 1080p",
                "VC1 Smooth Streaming 720p",
                "VC1 Smooth Streaming SD 16x9",
                "VC1 Smooth Streaming SD 4x3",
                "VC1 Smooth Streaming 1080p Xbox Live ADK",
                "VC1 Smooth Streaming 720p Xbox Live ADK",
                "H264 Broadband 1080p",
                "H264 Broadband 720p",
                "H264 Broadband SD 16x9",
                "H264 Broadband SD 4x3",
                "H264 Smooth Streaming 1080p",
                "H264 Smooth Streaming 720p",
                "H264 Smooth Streaming 720p for 3G or 4G",
                "H264 Smooth Streaming SD 16x9",
                "H264 Smooth Streaming SD 4x3",
                "H264 Adaptive Bitrate MP4 Set 1080p",
                "H264 Adaptive Bitrate MP4 Set 720p",
                "H264 Adaptive Bitrate MP4 Set SD 16x9",
                "H264 Adaptive Bitrate MP4 Set SD 4x3",
                "H264 Adaptive Bitrate MP4 Set 1080p for iOS Cellular Only",
                "H264 Adaptive Bitrate MP4 Set 720p for iOS Cellular Only",
                "H264 Adaptive Bitrate MP4 Set SD 16x9 for iOS Cellular Only",
                "H264 Adaptive Bitrate MP4 Set SD 4x3 for iOS Cellular Only",
                "H264 Smooth Streaming 720p Xbox Live ADK",
                "H264 Smooth Streaming Windows Phone 7 Series"
            };
            settings.SubtitleLanguages = new[] {
                "da-DK",
                "nl-BE",
                "nl-NL",
                "en-AU",
                "en-CA",
                "en-IE",
                "en-NZ",
                "en-GB",
                "en-US",
                "fr-BE",
                "fr-CA",
                "fr-FR",
                "fr-CH",
                "de-AT",
                "de-DE",
                "de-CH",
                "ga-IE",
                "it-IT",
                "it-CH",
                "nb-NO",
                "nn-NO",
                "fa-IR",
                "pl-PL",
                "pt-BR",
                "pt-PT",
                "ru-RU",
                "es-CO",
                "es-MX",
                "es-ES",
                "sv-SE"
            };
        }

        public void Enabling(Feature feature) {

        }

        public void Enabled(Feature feature) {

        }

        public void Disabling(Feature feature) {

        }

        public void Disabled(Feature feature) {

        }

        public void Uninstalling(Feature feature) {

        }

        public void Uninstalled(Feature feature) {

        }
    }
}