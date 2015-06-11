using System.Collections.Generic;
using System.Linq;
using Orchard.AuditTrail.Services;
using Orchard.AuditTrail.Shapes;
using Orchard.AuditTrail.ViewModels;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment;
using Orchard.Logging;

namespace Orchard.AuditTrail.Providers.AuditTrail {
    public class AuditTrailSettingsEventShape : AuditTrailEventShapeAlteration<AuditTrailSettingsEventProvider> {
        private readonly Work<IAuditTrailManager> _auditTrailManager;

        public AuditTrailSettingsEventShape(Work<IAuditTrailManager> auditTrailManager) {
            _auditTrailManager = auditTrailManager;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        protected override string EventName {
            get { return AuditTrailSettingsEventProvider.EventsChanged; }
        }

        protected override void OnAlterShape(ShapeDisplayingContext context) {
            var eventData = (IDictionary<string, object>)context.Shape.EventData;
            var oldSettings = AuditTrailManagerExtensions.DeserializeEventData((string)eventData["OldSettings"], Logger);
            var newSettings = AuditTrailManagerExtensions.DeserializeEventData((string)eventData["NewSettings"], Logger);
            var diff = GetDiffQuery(oldSettings, newSettings).ToArray();

            context.Shape.OldSettings = oldSettings;
            context.Shape.NewSettings = newSettings;
            context.Shape.Diff = diff;
        }

        private IEnumerable<AuditTrailEventDescriptorSettingViewModel> GetDiffQuery(IEnumerable<AuditTrailEventSettingEventData> oldSettings, IEnumerable<AuditTrailEventSettingEventData> newSettings) {
            var oldDictionary = oldSettings.ToDictionary(x => x.EventName);

            return
                from newSetting in newSettings
                let oldSetting = oldDictionary.ContainsKey(newSetting.EventName) ? oldDictionary[newSetting.EventName] : default(AuditTrailEventSettingEventData)
                where oldSetting == null || oldSetting.IsEnabled != newSetting.IsEnabled
                let descriptor = _auditTrailManager.Value.DescribeEvent(newSetting.EventName)
                select new AuditTrailEventDescriptorSettingViewModel {
                    Setting = newSetting,
                    Descriptor = descriptor
                };
        }
    }
}