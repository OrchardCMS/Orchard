using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement.MetaData.Models;

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