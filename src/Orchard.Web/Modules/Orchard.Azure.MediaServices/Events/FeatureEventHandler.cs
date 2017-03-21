﻿using System.Linq;
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
            settings.EnableDynamicPackaging = true;
            settings.DefaultWamsEncodingPresetIndex = 0;
            settings.WamsEncodingPresets = new[] {
                "Adaptive Streaming",
                "H264 Multiple Bitrate 1080p Audio 5.1",
                "H264 Multiple Bitrate 1080p",
                "H264 Multiple Bitrate 16x9 for iOS",
                "H264 Multiple Bitrate 16x9 SD Audio 5.1",
                "H264 Multiple Bitrate 16x9 SD",
                "H264 Multiple Bitrate 4K Audio 5.1",
                "H264 Multiple Bitrate 4K",
                "H264 Multiple Bitrate 4x3 for iOS",
                "H264 Multiple Bitrate 4x3 SD Audio 5.1",
                "H264 Multiple Bitrate 4x3 SD",
                "H264 Multiple Bitrate 720p Audio 5.1",
                "H264 Multiple Bitrate 720p",
                "H264 Single Bitrate 1080p Audio 5.1",
                "H264 Single Bitrate 1080p",
                "H264 Single Bitrate 4K Audio 5.1",
                "H264 Single Bitrate 4K",
                "H264 Single Bitrate 4x3 SD Audio 5.1",
                "H264 Single Bitrate 4x3 SD",
                "H264 Single Bitrate 16x9 SD Audio 5.1",
                "H264 Single Bitrate 16x9 SD",
                "H264 Single Bitrate 720p Audio 5.1",
                "H264 Single Bitrate 720p for Android",
                "H264 Single Bitrate 720p",
                "H264 Single Bitrate High Quality SD for Android",
                "H264 Single Bitrate Low Quality SD for Android"
            }.Select(x => new EncodingPreset() { Name = x });
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