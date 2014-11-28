namespace Orchard.AuditTrail.Providers.AuditTrail {
    public class AuditTrailEventSettingEventData
    {
        public string EventName { get; set; }
        public string EventDisplayName { get; set; }
        public string EventCategory { get; set; }
        public bool IsEnabled { get; set; }
    }
}