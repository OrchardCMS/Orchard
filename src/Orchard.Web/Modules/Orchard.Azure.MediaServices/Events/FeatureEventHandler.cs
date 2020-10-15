using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Orchard.Azure.MediaServices.Helpers;
using Orchard.Azure.MediaServices.Models;
using Orchard.ContentManagement;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Azure.MediaServices.Events {
    public class FeatureEventHandler : IFeatureEventHandler {

        private const string AppSettingsKeyAccountName = "Azure.MediaServices.AccountName";
        private const string AppSettingsKeyAccountKey = "Azure.MediaServices.AccountKey";
        private const string AppSettingsKeyStorageAccountKey = "Azure.MediaServices.StorageAccountKey";
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

            if (ConfigurationManager.AppSettings[AppSettingsKeyAccountName] != null)
            {
                settings.WamsAccountName = ConfigurationManager.AppSettings[AppSettingsKeyAccountName];
            }

            if (ConfigurationManager.AppSettings[AppSettingsKeyAccountKey] != null)
            {
                settings.WamsAccountKey = ConfigurationManager.AppSettings[AppSettingsKeyAccountKey];
            }

            if (ConfigurationManager.AppSettings[AppSettingsKeyStorageAccountKey] != null)
            {
                settings.StorageAccountKey = ConfigurationManager.AppSettings[AppSettingsKeyStorageAccountKey];
            }

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

            try
            {
                var originsToAdd = new List<string>();
                var baseUrlOrigin = new Uri(_orchardServices.WorkContext.CurrentSite.BaseUrl).GetLeftPart(UriPartial.Authority);
                originsToAdd.Add(baseUrlOrigin);

                var currentUrlOrigin = _orchardServices.WorkContext.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                if (!originsToAdd.Contains(currentUrlOrigin))
                    originsToAdd.Add(currentUrlOrigin);

                StorageHelper.EnsureCorsIsEnabledAsync(settings.WamsAccountName, settings.WamsAccountKey, settings.StorageAccountKey, originsToAdd.ToArray()).Wait();
            }
            catch
            {
            }
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