using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Helpers;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Providers.AuditTrail;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Services.Models;
using Orchard.AuditTrail.ViewModels;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;

namespace Orchard.AuditTrail.Shapes {
    public class AuditTrailSettingsEventShape : IShapeTableProvider {
        private readonly Work<IAuditTrailManager> _auditTrailManager;
        public AuditTrailSettingsEventShape(Work<IAuditTrailManager> auditTrailManager) {
            _auditTrailManager = auditTrailManager;
        }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("AuditTrailEvent").OnDisplaying(context => {
                var descriptor = (AuditTrailEventDescriptor) context.Shape.Descriptor;
                if (descriptor.Event != EventNameExtensions.GetFullyQualifiedEventName<SettingsAuditTrailEventProvider>(SettingsAuditTrailEventProvider.EventsChanged))
                    return;

                var eventData = (IDictionary<string, object>)context.Shape.EventData;
                var oldSettings = _auditTrailManager.Value.DeserializeProviderConfiguration((string)eventData["OldSettings"]);
                var newSettings = _auditTrailManager.Value.DeserializeProviderConfiguration((string)eventData["NewSettings"]);
                var diff = GetDiffQuery(oldSettings, newSettings).ToArray();

                context.Shape.OldSettings = oldSettings;
                context.Shape.NewSettings = newSettings;
                context.Shape.Diff = diff;
            });
        }

        private IEnumerable<AuditTrailEventDescriptorSettingViewModel> GetDiffQuery(IEnumerable<AuditTrailEventSetting> oldSettings, IEnumerable<AuditTrailEventSetting> newSettings) {
            var oldDictionary = oldSettings.ToDictionary(x => x.EventName);
            
            return from newSetting in newSettings
                   let oldSetting = oldDictionary.ContainsKey(newSetting.EventName) ? oldDictionary[newSetting.EventName] : default(AuditTrailEventSetting) 
                   where oldSetting == null || oldSetting.IsEnabled != newSetting.IsEnabled
                   select new AuditTrailEventDescriptorSettingViewModel {
                       Setting = newSetting,
                       Descriptor = _auditTrailManager.Value.DescribeEvent(newSetting.EventName)
                   };
        }
    }
}