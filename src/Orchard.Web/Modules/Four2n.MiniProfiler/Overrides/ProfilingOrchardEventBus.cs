using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Exceptions;
using Orchard.Localization;

using Orchard.Events;
using Orchard.Environment.Extensions;
using Four2n.Orchard.MiniProfiler.Services;

namespace  Four2n.Orchard.MiniProfiler.Overrides {
    [OrchardSuppressDependency("Orchard.Events.DefaultOrchardEventBus")]
        public class DefaultOrchardEventBus : IEventBus {
            private readonly Func<IEnumerable<IEventHandler>> _eventHandlers;
            private readonly IExceptionPolicy _exceptionPolicy;
            private static readonly ConcurrentDictionary<string, MethodInfo> _interfaceMethodsCache = new ConcurrentDictionary<string, MethodInfo>();  
            private readonly IProfilerService _profiler;
        public DefaultOrchardEventBus(Func<IEnumerable<IEventHandler>> eventHandlers, IExceptionPolicy exceptionPolicy, IProfilerService profiler) {
                _eventHandlers = eventHandlers;
                _exceptionPolicy = exceptionPolicy;
                _profiler = profiler;
                T = NullLocalizer.Instance;
            }


            public Localizer T { get; set; }


            public IEnumerable Notify(string messageName, IDictionary<string, object> eventData) {
                // NOTE: We can't profile everything because EventsInterceptor performs some work that's a bit harder to profile without forking or getting our
                // own interceptor working...
                _profiler.StepStart("EventBusNotify","EventBus: "+messageName);
                // call ToArray to ensure evaluation has taken place
                var result = NotifyHandlers(messageName, eventData).ToArray();
                _profiler.StepStop("EventBusNotify");
                return result;
            }

        private IEnumerable<object> NotifyHandlers(string messageName, IDictionary<string, object> eventData) {
                string[] parameters = messageName.Split('.');
                if (parameters.Length != 2) {
                    throw new ArgumentException(T("{0} is not formatted correctly", messageName).Text);
                }
                string interfaceName = parameters[0];
                string methodName = parameters[1];

                var eventHandlers = _eventHandlers();
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

        private bool TryNotifyHandler(IEventHandler eventHandler, string messageName, string interfaceName, string methodName, IDictionary<string, object> eventData, out IEnumerable returnValue) {
            try {
                    return TryInvoke(eventHandler, interfaceName, methodName, eventData, out returnValue, _profiler);
                }
            catch (Exception exception) {
                if (!_exceptionPolicy.HandleException(this, exception)) {
                    throw;
                }

                returnValue = null;
                return false;
            }
        }

        private static bool TryInvoke(IEventHandler eventHandler, string interfaceName, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue, IProfilerService profiler) {
            Type type = eventHandler.GetType();
                foreach (var interfaceType in type.GetInterfaces()) {
                    if (String.Equals(interfaceType.Name, interfaceName, StringComparison.OrdinalIgnoreCase)) {
                        return TryInvokeMethod(eventHandler, interfaceType, methodName, arguments, out returnValue, profiler);
                    }
                }
                returnValue = null;
                return false;
            }

        private static bool TryInvokeMethod(IEventHandler eventHandler, Type interfaceType, string methodName, IDictionary<string, object> arguments, out IEnumerable returnValue, IProfilerService profiler) {
            MethodInfo method = _interfaceMethodsCache.GetOrAdd(String.Concat(eventHandler.GetType().Name + "_" + interfaceType.Name, "_", methodName, "_", String.Join("_", arguments.Keys)), GetMatchingMethod(eventHandler, interfaceType, methodName, arguments));

                if (method != null) {
                    var parameters = new List<object>();
                    foreach (var methodParameter in method.GetParameters()) {
                        parameters.Add(arguments[methodParameter.Name]);
                    }
                    var key= "EventBus:"+eventHandler.GetType().FullName +"."+ methodName;
                    profiler.StepStart(key,String.Format("EventBus: {0}",eventHandler.GetType().FullName +"."+ methodName),true);
                    var result = method.Invoke(eventHandler, parameters.ToArray());
                    profiler.StepStop(key);
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
