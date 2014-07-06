using System;
using System.Collections.Generic;

namespace Orchard.AuditTrail.Helpers {
    public static class EventDataExtensions {
        public static T Get<T>(this IDictionary<string, object> eventData, string key) {
            if (eventData == null || !eventData.ContainsKey(key))
                return default(T);

            var value = eventData[key];

            return (T) Convert.ChangeType(value, typeof (T));
        }
    }
}