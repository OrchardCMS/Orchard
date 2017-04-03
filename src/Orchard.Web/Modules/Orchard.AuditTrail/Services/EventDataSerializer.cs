using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Newtonsoft.Json;
using Orchard.Logging;

namespace Orchard.AuditTrail.Services {

    /// <summary>
    /// A service responsible for serializing and deserializing audit trail event data.
    /// </summary>
    public class EventDataSerializer : Component, IEventDataSerializer {

        /// <summary>
        /// Serialize event data.
        /// </summary>
        /// <param name="eventData">eventData to be serialized.</param>
        /// <returns>The serialized data.</returns>
        public string Serialize(IDictionary<string, object> eventData) {
            try {
                var json = JsonConvert.SerializeObject(eventData, Formatting.None);
                var xml = JsonConvert.DeserializeXNode(json, deserializeRootElementName: "EventData");
                return xml.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error during serialization of event data.");
            }
            return null;
        }

        /// <summary>
        /// Deserialize event data.
        /// </summary>
        /// <param name="eventData">eventData to be deserialized.</param>
        /// <returns>The deserialized generic dictionary</returns>
        public IDictionary<string, object> Deserialize(string eventData) {
            if (String.IsNullOrWhiteSpace(eventData))
                return new Dictionary<string, object>();

            try {
                var node = XDocument.Parse(eventData);
                var json = JsonConvert.SerializeXNode(node, Formatting.None, omitRootObject: true);
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }
            catch (Exception ex) {
                Logger.Error(ex, "Error during deserialization of event data.");
            }
            return new Dictionary<string, object>();
        }
    }
}