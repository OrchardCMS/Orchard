using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Models;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Shapes;
using Orchard.AuditTrail.ViewModels;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;

namespace Orchard.AuditTrail.Providers.AuditTrail {
    public class AuditTrailSettingsEventShape : AuditTrailEventShapeAlteration<SettingsAuditTrailEventProvider> {
        private readonly Work<IAuditTrailManager> _auditTrailManager;
        public AuditTrailSettingsEventShape(Work<IAuditTrailManager> auditTrailManager) {
            _auditTrailManager = auditTrailManager;
        }

        protected override string EventName {
            get { return SettingsAuditTrailEventProvider.EventsChanged; }
        }

        protected override void OnAlterShape(ShapeDisplayingContext context) {
            var eventData = (IDictionary<string, object>)context.Shape.EventData;
            var oldSettings = _auditTrailManager.Value.DeserializeProviderConfiguration((string)eventData["OldSettings"]);
            var newSettings = _auditTrailManager.Value.DeserializeProviderConfiguration((string)eventData["NewSettings"]);
            var diff = GetDiffQuery(oldSettings, newSettings).ToArray();

            context.Shape.OldSettings = oldSettings;
            context.Shape.NewSettings = newSettings;
            context.Shape.Diff = diff;
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