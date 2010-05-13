using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly Func<IEnumerable<IEventBusHandler>> _handlers;
        private readonly Func<IEnumerable<IEventHandler>> _eventHandlers;

        public DefaultOrchardEventBus(Func<IEnumerable<IEventBusHandler>> handlers, Func<IEnumerable<IEventHandler>> eventHandlers) {
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

            foreach (var eventHandler in _eventHandlers()) {
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
            MethodInfo method = GetMatchingMethod(eventHandler, methodName, arguments);
            if (method != null) {
                List<object> parameters = new List<object>();
                foreach (var methodParameter in method.GetParameters()) {
                    parameters.Add(arguments[methodParameter.Name]);
                }
                method.Invoke(eventHandler, parameters.ToArray());
            }
        }

        private static MethodInfo GetMatchingMethod(IEventHandler eventHandler, string methodName, IDictionary<string, object> arguments) {            
            List<MethodInfo> allMethods = new List<MethodInfo>(eventHandler.GetType().GetMethods());
            List<MethodInfo> candidates = new List<MethodInfo>(allMethods);

            foreach (var method in allMethods) {
                if (String.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase)) {
                    ParameterInfo[] parameterInfos = method.GetParameters();
                    foreach (var parameter in parameterInfos) {
                        if (!arguments.ContainsKey(parameter.Name)) {
                            candidates.Remove(method);
                            break;
                        }
                    }
                }
                else {
                    candidates.Remove(method);
                }
            }

            if (candidates.Count != 0) {
                return candidates.OrderBy(x => x.GetParameters().Length).Last();
            }

            return null;
        }

        #endregion
    }
}
