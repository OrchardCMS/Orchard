using System;
using System.Collections.Generic;
using Orchard.Logging;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly Func<IEnumerable<IEventBusHandler>> _handlers;
        private readonly IEnumerable<IEventHandler> _eventHandlers;

        public DefaultOrchardEventBus(Func<IEnumerable<IEventBusHandler>> handlers, IEnumerable<IEventHandler> eventHandlers) {
            _handlers = handlers;
            _eventHandlers = eventHandlers;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        #region Implementation of IEventBus

        public void Notify_Obsolete(string messageName, IDictionary<string, string> eventData) {
            _handlers().Invoke(handler => handler.Process(messageName, eventData), Logger);
        }

        public void Notify(string messageName, Dictionary<string, object> eventData) {
        }

        #endregion
    }
}
