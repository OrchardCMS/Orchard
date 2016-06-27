using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Orchard.AuditTrail.Helpers {
    public static class EventDataExtensions {
        public static T Get<T>(this IDictionary<string, object> eventData, string key) {
            if (eventData == null || !eventData.ContainsKey(key))
                return default(T);

            var value = eventData[key];

            return (T) Convert.ChangeType(value, typeof (T));
        }

        public static XElement GetXml(this IDictionary<string, object> eventData, string key) {
            var data = eventData.Get<string>(key);

            if (String.IsNullOrWhiteSpace(data))
                return null;

            try {
                return XElement.Parse(data);
            }
            catch (Exception) {
                return null;
            }
        }
    }
}