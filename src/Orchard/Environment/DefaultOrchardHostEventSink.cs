using System.Collections.Generic;
using Orchard.Events;

namespace Orchard.Environment {
    /// <summary>
    /// This handler forwards calls to the IOrchardHost when it is an instance of DefaultOrchardHost.
    /// The reason for this is to avoid adding IEventBusHandler, because DefaultOrchardHost is a component
    /// that should not be detected and registererd automatically as an IDependency.
    /// </summary>
    public class DefaultOrchardHostEventSink : IEventBusHandler {
        private readonly DefaultOrchardHost _host;

        public DefaultOrchardHostEventSink(IOrchardHost host) {
            _host = host as DefaultOrchardHost;
        }

        public void Process(string messageName, IDictionary<string, string> eventData) {
            if (_host != null) {
                _host.Process(messageName, eventData);
            }
        }
    }
}
