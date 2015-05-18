﻿using System.Collections.Generic;
using System.Globalization;
using Orchard.ContentManagement;
using System;
using System.Linq;
using Newtonsoft.Json;

namespace Orchard.Azure.MediaServices.Models {

    public class CloudMediaSettingsPart : ContentPart {

        public string WamsAccountName {
            get { return this.Retrieve(x => x.WamsAccountName); }
            set { this.Store(x => x.WamsAccountName, value); }
        }

        public string WamsAccountKey {
            get { return this.Retrieve(x => x.WamsAccountKey); }
            set { this.Store(x => x.WamsAccountKey, value); }
        }

        public string StorageAccountKey {
            get { return this.Retrieve(x => x.StorageAccountKey); }
            set { this.Store(x => x.StorageAccountKey, value); }
        }

        public bool EnableDynamicPackaging {
            get { return this.Retrieve(x => x.EnableDynamicPackaging); }
            set { this.Store(x => x.EnableDynamicPackaging, value); }
        }

        public TimeSpan AccessPolicyDuration {
            get {
                var duration = Retrieve<string>("AccessPolicyDuration");
                return !String.IsNullOrEmpty(duration) ? TimeSpan.Parse(duration, CultureInfo.InvariantCulture) : TimeSpan.FromDays(365 * 5);
            }
            set {
                Store("AccessPolicyDuration", value.ToString());
            }
        }

        public IEnumerable<string> AllowedVideoFilenameExtensions {
            get {
                var languages = Retrieve<string>("AllowedVideoFilenameExtensions");
                return !String.IsNullOrEmpty(languages) ? languages.Split(';') : new string[] { };
            }
            set {
                var languages = value != null && value.Any() ? String.Join(";", value) : null;
                Store("AllowedVideoFilenameExtensions", languages);
            }
        }

        public IEnumerable<EncodingPreset> WamsEncodingPresets {
            get {
                var presets = Retrieve<string>("WamsEncodingPresetsJson");
                if (!String.IsNullOrEmpty(presets)) {
                    return JsonConvert.DeserializeObject<EncodingPreset[]>(presets);
                }
                // Fall back to old style property for compatibility with existing data.
                presets = Retrieve<string>("WamsEncodingPresets");
                var presetNames = !String.IsNullOrEmpty(presets) ? presets.Split(';') : new string[] { };
                return presetNames.Select(x => new EncodingPreset() { Name = x });
            }
            set {
                // Clear out old style property, migrate setting to the new one.
                Store("WamsEncodingPresets", default(string));
                Store("WamsEncodingPresetsJson", JsonConvert.SerializeObject(value.ToArray()));
            }
        }

        public int DefaultWamsEncodingPresetIndex {
            get { return this.Retrieve(x => x.DefaultWamsEncodingPresetIndex, 11); }
            set { this.Store(x => x.DefaultWamsEncodingPresetIndex, value); }
        }

        public string EncryptionKeySeedValue {
            get { return this.Retrieve(x => x.EncryptionKeySeedValue); }
            set { this.Store(x => x.EncryptionKeySeedValue, value); }
        }

        public string EncryptionLicenseAcquisitionUrl {
            get { return this.Retrieve(x => x.EncryptionLicenseAcquisitionUrl); }
            set { this.Store(x => x.EncryptionLicenseAcquisitionUrl, value); }
        }

        public IEnumerable<string> SubtitleLanguages {
            get {
                var languages = Retrieve<string>("SubtitleLanguages");
                return !String.IsNullOrEmpty(languages) ? languages.Split(';') : new string[] { };
            }
            set {
                var languages = value != null && value.Any() ? String.Join(";", value) : null;
                Store("SubtitleLanguages", languages);
            }
        }

        public bool IsValid() {
            if (String.IsNullOrWhiteSpace(WamsAccountKey) || String.IsNullOrWhiteSpace(WamsAccountName))
                return false;
            if (!AllowedVideoFilenameExtensions.Any())
                return false;
            if (!WamsEncodingPresets.Any())
                return false;
            if (DefaultWamsEncodingPresetIndex < 0 || DefaultWamsEncodingPresetIndex > WamsEncodingPresets.Count() - 1)
                return false;

            return true;
        }
    }
}