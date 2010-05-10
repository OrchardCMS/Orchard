using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBus : IDependency {
        void Notify_Obsolete(string messageName, IDictionary<string, string> eventData);
        void Notify(string messageName, Dictionary<string, object> eventData);
    }
}
