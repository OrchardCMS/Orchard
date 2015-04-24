using System;
using Orchard.AuditTrail.Services;

namespace Orchard.AuditTrail.Helpers {
    public static class EventNameExtensions {
        public static string GetFullyQualifiedEventName<T>(string eventName) where T : IAuditTrailEventProvider {
            return GetFullyQualifiedEventName(typeof(T), eventName);
        }

        public static string GetFullyQualifiedEventName(Type providerType, string eventName) {
            return String.Format("{0}.{1}", providerType.FullName, eventName);
        }

        public static string GetShortEventName(string fullyQualifiedEventName) {
            var index = fullyQualifiedEventName.LastIndexOf('.') + 1;
            return fullyQualifiedEventName.Substring(index);
        }
    }
}