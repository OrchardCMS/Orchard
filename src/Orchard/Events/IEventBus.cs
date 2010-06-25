using System.Collections;
using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBus : IDependency {
        IEnumerable Notify(string messageName, Dictionary<string, object> eventData);
    }
}
