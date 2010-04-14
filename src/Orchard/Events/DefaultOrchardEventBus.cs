using System.Collections.Generic;
using Orchard.Logging;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly IEnumerable<IEventBusHandler> _handlers;

        public DefaultOrchardEventBus(IEnumerable<IEventBusHandler> handlers) {
            _handlers = handlers;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of IEventBus

        public void Notify(string messageName, IDictionary<string, string> eventData) {
            _handlers.Invoke(handler => handler.Process(messageName, eventData), Logger);
        }

        #endregion
    }
}
