using System.Collections.Generic;

namespace Orchard.AuditTrail.Services {
    /// <summary>
    /// Serialize or deserialize event data.
    /// </summary>
    public interface IEventDataSerializer : IDependency {
        /// <summary>
        /// Serialize event data.
        /// </summary>
        /// <param name="eventData">eventData to be serialized.</param>
        /// <returns>The serialized data.</returns>
        string Serialize(IDictionary<string, object> eventData);

        /// <summary>
        /// Deserialize event data.
        /// </summary>
        /// <param name="eventData">eventData to be deserialized.</param>
        /// <returns>The deserialized generic dictionary</returns>
        IDictionary<string, object> Deserialize(string eventData);
    }
}