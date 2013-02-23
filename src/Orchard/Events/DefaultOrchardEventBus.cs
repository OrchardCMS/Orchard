using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
using Orchard.Exceptions;
using Orchard.Localization;

namespace Orchard.Events {
    public class DefaultOrchardEventBus : IEventBus {
        private readonly IIndex<string, IEnumerable<Meta<IEventHandler>>> _eventHandlers;
        private readonly IExceptionPolicy _exceptionPolicy;
        private static readonly ConcurrentDictionary<string, Tuple<ParameterInfo[], Func<IEventHandler, object[], object>>> _delegateCache = new ConcurrentDictionary<string, Tuple<ParameterInfo[], Func<IEventHandler, object[], object>>>();

        public DefaultOrchardEventBus(IIndex<string, IEnumerable<Meta<IEventHandler>>> eventHandlers, IExceptionPolicy exceptionPolicy)
        {
            _eventHandlers = eventHandlers;
            _exceptionPolicy = exceptionPolicy;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable Notify(string messageName, IDictionary<string, object> eventData) {
            // call ToArray to ensure evaluation has taken place
            return NotifyHandlers(messageName, eventData).ToArray();
        }

        private IEnumerable<object> NotifyHandlers(string messageName, IDictionary<string, object> eventData) {
            string[] parameters = messageName.Split('.');
            if (parameters.Length != 2) {
                throw new ArgumentException(T("{0} is not formatted correctly", messageName).Text);
            }
            string interfaceName = parameters[0];
            string methodName = parameters[1];

            var eventHandlers = _eventHandlers[interfaceName];
            foreach (var eventHandler in eventHandlers) {
                IEnumerable returnValue;
                if (TryNotifyHandler(eventHandler, messageName, interfaceName, methodName, eventData, out returnValue)) {
                    if (returnValue != null) {
                        foreach (var value in returnValue) {
                            yield return value;
                        }
                    }
                }
            }
        }

        private bool TryNotifyHandler(Meta<IEventHandler> eventHandler, string messageName, string interfaceName, string methodName, IDictionary<string, object> eventData, out IEnumerable returnValue) {
            try {
                return TryInvoke(eventHandler, interfaceName, methodName, eventData, out returnValue);
            }
            catch (Exception exception) {
                if (!_exceptionPolicy.HandleException(this, exception)) {
                    throw;
                }

                returnValue = null;
                return false;
            }
        }

        private static bool TryInvoke(Meta<IEventHandler> eventHandler, string interfaceName, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue) {
            var matchingInterfaces = ((ILookup<string, Type>)eventHandler.Metadata["Interfaces"])[interfaceName];
            foreach (var interfaceType in matchingInterfaces) {
                return TryInvokeMethod(eventHandler.Value, interfaceType, methodName, arguments, out returnValue);
            }
            returnValue = null;
            return false;
        }

        private static bool TryInvokeMethod(IEventHandler eventHandler, Type interfaceType, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue) {
            var key = eventHandler.GetType().FullName + "_" + interfaceType.Name + "_" + methodName + "_" + String.Join("_", arguments.Keys);
            var cachedDelegate = _delegateCache.GetOrAdd(key, k => {
                var method = GetMatchingMethod(eventHandler, interfaceType, methodName, arguments);
                return method != null 
                    ? Tuple.Create(method.GetParameters(), DelegateHelper.CreateDelegate<IEventHandler>(interfaceType, method))
                    : null;
            });

            if (cachedDelegate != null) {
                var args = cachedDelegate.Item1.Select(methodParameter => arguments[methodParameter.Name]).ToArray();
                var result = cachedDelegate.Item2(eventHandler, args);

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
