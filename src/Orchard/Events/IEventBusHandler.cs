using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBusHandler : IDependency {
        void Process(string messageName, IDictionary<string, string> eventData);
    }
}
