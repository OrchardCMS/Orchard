using System;
using System.Xml.Linq;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;

namespace Orchard.AuditTrail.Helpers {
    public static class SettingsDictionaryExtensions {

        public static string Get(this SettingsDictionary settings, string key) {
            return settings.ContainsKey(key) ? settings[key] : null;
        }

        public static DiffDictionary<string, string> GetDiff(this SettingsDictionary oldSettings, SettingsDictionary newSettings) {
            var dictionary = new DiffDictionary<string, string>();

            BuildDiff(dictionary, newSettings, oldSettings);
            BuildDiff(dictionary, oldSettings, newSettings);

            return dictionary;
        }

        public static bool IsEqualTo(this SettingsDictionary a, SettingsDictionary b, ISettingsFormatter settingsFormatter) {
            var xml1 = ToXml(a, settingsFormatter);
            var xml2 = ToXml(b, settingsFormatter);

            return String.Equals(xml1, xml2, StringComparison.OrdinalIgnoreCase);
        }

        public static string ToXml(this SettingsDictionary settings, ISettingsFormatter settingsFormatter) {
            return settingsFormatter.Map(settings).ToString(SaveOptions.DisableFormatting);
        }

        private static void BuildDiff(DiffDictionary<string, string> dictionary, SettingsDictionary settingsA, SettingsDictionary settingsB) {

            foreach (var settingA in settingsA) {
                string oldValue, newValue;

                if (settingsB.ContainsKey(settingA.Key)) {
                    oldValue = settingA.Value;
                    newValue = settingsB[settingA.Key];
                }
                else {
                    oldValue = settingA.Value;
                    newValue = settingsB[settingA.Key] = default(string);

                }

                if (oldValue != newValue) {
                    dictionary[settingA.Key] = new Diff<string> {
                        NewValue = newValue,
                        OldValue = oldValue
                    };
                }
            }
        }
    }
}