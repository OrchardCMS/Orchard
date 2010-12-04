using System.Collections;
using System.Collections.Generic;

namespace Orchard.Events {
    public interface IEventBus : IDependency {
        IEnumerable Notify(string messageName, IDictionary<string, object> eventData);
        IEnumerable NotifyFailFast(string messageName, IDictionary<string, object> eventData);
    }
}
