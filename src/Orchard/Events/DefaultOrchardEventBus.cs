using System;
using System.Collections.Generic;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly Func<IEnumerable<IEventBusHandler>> _handlers;
        private readonly IEnumerable<IEventHandler> _eventHandlers;

        public DefaultOrchardEventBus(Func<IEnumerable<IEventBusHandler>> handlers, IEnumerable<IEventHandler> eventHandlers) {
            _handlers = handlers;
            _eventHandlers = eventHandlers;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        #region Implementation of IEventBus

        public void Notify_Obsolete(string messageName, IDictionary<string, string> eventData) {
            _handlers().Invoke(handler => handler.Process(messageName, eventData), Logger);
        }

        public void Notify(string messageName, Dictionary<string, object> eventData) {
            string[] parameters = messageName.Split('.');
            if (parameters.Length != 2) {
                throw new ArgumentException(messageName + T(" is not formatted correctly"));
            }
            string interfaceName = parameters[0];
            string methodName = parameters[1];

            foreach (var eventHandler in _eventHandlers) {
                TryInvoke(eventHandler, interfaceName, methodName, eventData);
            }
        }

        private static void TryInvoke(IEventHandler eventHandler, string interfaceName, string methodName, IDictionary<string, object> arguments) {
            Type type = eventHandler.GetType();
            foreach (var interfaceType in type.GetInterfaces()) {
                if (String.Equals(interfaceType.Name, interfaceName, StringComparison.OrdinalIgnoreCase)) {
                    TryInvokeMethod(eventHandler, methodName, arguments);
                    break;
                }
            }
        }

        private static void TryInvokeMethod(IEventHandler eventHandler, string methodName, IDictionary<string, object> arguments) {
            foreach (var method in eventHandler.GetType().GetMethods()) {
                if (String.Equals(method.Name, methodName)) {
                    List<object> parameters = new List<object>();
                    foreach (var methodParameter in method.GetParameters()) {
                        if (arguments.ContainsKey(methodParameter.Name)) {
                            parameters.Add(arguments[methodParameter.Name]);
                        }
                        else {
                            parameters.Add(new object());
                        }
                    }
                    method.Invoke(eventHandler, parameters.ToArray());
                    break;
                }
            }
        }

        #endregion
    }
}
