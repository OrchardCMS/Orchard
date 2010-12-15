using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly Func<IEnumerable<IEventHandler>> _eventHandlers;

        public DefaultOrchardEventBus(Func<IEnumerable<IEventHandler>> eventHandlers) {
            _eventHandlers = eventHandlers;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public IEnumerable Notify(string messageName, IDictionary<string, object> eventData) {
            // call ToArray to ensure evaluation has taken place
            return NotifyHandlers(messageName, eventData, true/*failFast*/).ToArray();
        }

        private IEnumerable<object> NotifyHandlers(string messageName, IDictionary<string, object> eventData, bool failFast) {
            string[] parameters = messageName.Split('.');
            if (parameters.Length != 2) {
                throw new ArgumentException(T("{0} is not formatted correctly", messageName).Text);
            }
            string interfaceName = parameters[0];
            string methodName = parameters[1];

            var eventHandlers = _eventHandlers();
            foreach (var eventHandler in eventHandlers) {
                IEnumerable returnValue;
                if (TryNotifyHandler(eventHandler, messageName, interfaceName, methodName, eventData, failFast, out returnValue)) {
                    if (returnValue != null) {
                        foreach (var value in returnValue) {
                            yield return value;
                        }
                    }
                }
            }
        }

        private bool TryNotifyHandler(IEventHandler eventHandler, string messageName, string interfaceName, string methodName, IDictionary<string, object> eventData, bool failFast, out IEnumerable returnValue) {
            try {
                return TryInvoke(eventHandler, interfaceName, methodName, eventData, out returnValue);
            }
            catch (Exception ex) {
                Logger.Error(ex, "{2} thrown from {0} by {1}",
                             messageName,
                             eventHandler.GetType().FullName,
                             ex.GetType().Name);

                if (failFast)
                    throw;

                returnValue = null;
                return false;
            }
        }

        private static bool TryInvoke(IEventHandler eventHandler, string interfaceName, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue) {
            Type type = eventHandler.GetType();
            foreach (var interfaceType in type.GetInterfaces()) {
                if (String.Equals(interfaceType.Name, interfaceName, StringComparison.OrdinalIgnoreCase)) {
                    return TryInvokeMethod(eventHandler, interfaceType, methodName, arguments, out returnValue);
                }
            }
            returnValue = null;
            return false;
        }

        private static bool TryInvokeMethod(IEventHandler eventHandler, Type interfaceType, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue) {
            MethodInfo method = GetMatchingMethod(eventHandler, interfaceType, methodName, arguments);
            if (method != null) {
                var parameters = new List<object>();
                foreach (var methodParameter in method.GetParameters()) {
                    parameters.Add(arguments[methodParameter.Name]);
                }
                var result = method.Invoke(eventHandler, parameters.ToArray());
                returnValue = result as IEnumerable;
                if (returnValue == null && result != null)
                    returnValue = new[] { result };
                return true;
            }
            returnValue = null;
            return false;
        }

        private static MethodInfo GetMatchingMethod(IEventHandler eventHandler, Type interfaceType, string methodName, IDictionary<string, object> arguments) {
            var allMethods = new List<MethodInfo>(interfaceType.GetMethods());
            var candidates = new List<MethodInfo>(allMethods);

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
    }
}
