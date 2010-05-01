using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBusHandler : IEvents {
        void Process(string messageName, IDictionary<string, string> eventData);
    }
}
