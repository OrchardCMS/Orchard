using System.Collections.Generic;

namespace Orchard.AuditTrail.Services {
    public interface IEventDataSerializer : IDependency {
        string Serialize(IDictionary<string, object> eventData);
        IDictionary<string, object> Deserialize(string eventData);
    }
}