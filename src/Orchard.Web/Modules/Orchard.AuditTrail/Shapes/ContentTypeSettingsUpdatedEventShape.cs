using System.Collections.Generic;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Providers.ContentDefinition;
using Orchard.AuditTrail.Services.Models;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.MetaData.Services;
using Orchard.DisplayManagement.Implementation;

namespace Orchard.AuditTrail.Shapes {
    public class ContentTypeSettingsUpdatedEventShape : AuditTrailEventShapeAlteration<ContentTypeAuditTrailEventProvider> {
        private readonly ISettingsFormatter _settingsFormatter;
        public ContentTypeSettingsUpdatedEventShape(ISettingsFormatter settingsFormatter) {
            _settingsFormatter = settingsFormatter;
        }

        protected override string EventName {
            get { return ContentTypeAuditTrailEventProvider.TypeSettingsUpdated; }
        }

        protected override void OnAlterShape(ShapeDisplayingContext context) {
            var eventData = (IDictionary<string, object>)context.Shape.EventData;
            var oldSettings = _settingsFormatter.Map(XmlHelper.Parse((string)eventData["OldSettings"]));
            var newSettings = _settingsFormatter.Map(XmlHelper.Parse((string)eventData["NewSettings"]));
            var diff = GetDiff(oldSettings, newSettings);

            context.Shape.OldSettings = oldSettings;
            context.Shape.NewSettings = newSettings;
            context.Shape.Diff = diff;
        }

        private static DiffDictionary<string, string> GetDiff(SettingsDictionary oldSettings, SettingsDictionary newSettings) {
            var dictionary = new DiffDictionary<string, string>();

            BuildDiff(dictionary, newSettings, oldSettings);
            BuildDiff(dictionary, oldSettings, newSettings);

            return dictionary;
        }

        private static void BuildDiff(DiffDictionary<string, string> dictionary, SettingsDictionary settingsA, SettingsDictionary settingsB) {
            
            foreach (var settingA in settingsB) {
                var b = settingsA.ContainsKey(settingA.Key) ? settingsA[settingA.Key] : default(string);

                if (b != settingA.Value) {
                    dictionary[settingA.Key] = new Diff<string> {
                        NewValue = settingA.Value,
                        OldValue = b
                    };
                }
            }
        }
    }
}