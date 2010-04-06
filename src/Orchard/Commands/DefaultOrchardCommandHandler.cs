using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Orchard.Localization;

namespace Orchard.Commands {
    public abstract class DefaultOrchardCommandHandler : ICommandHandler {
        protected DefaultOrchardCommandHandler() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #region Implementation of ICommandHandler

        public void Execute(CommandContext context) {
            if (context.Switches != null && context.Switches.Count > 0) {
                foreach (var commandSwitch in context.Switches.AllKeys) {
                    PropertyInfo propertyInfo = GetType().GetProperty(commandSwitch);
                    if (propertyInfo == null) {
                        throw new InvalidOperationException(T("Switch : ") + commandSwitch + T(" was not found"));
                    }
                    if (propertyInfo.GetCustomAttributes(typeof(OrchardSwitchAttribute), false).Length == 0) {
                        throw new InvalidOperationException(T("A property of the name ") + commandSwitch + T(" exists but is not decorated with the OrchardSwitch attribute"));
                    }
                    string stringValue = context.Switches[commandSwitch];
                    if (propertyInfo.PropertyType.IsAssignableFrom(typeof(bool))) {
                        bool boolValue;
                        Boolean.TryParse(stringValue, out boolValue);
                        propertyInfo.SetValue(this, boolValue, null);
                    }
                    else if (propertyInfo.PropertyType.IsAssignableFrom(typeof(int))) {
                        int intValue;
                        Int32.TryParse(stringValue, out intValue);
                        propertyInfo.SetValue(this, intValue, null);
                    }
                    else if (propertyInfo.PropertyType.IsAssignableFrom(typeof(string))) {
                        propertyInfo.SetValue(this, stringValue, null);
                    }
                    else {
                        throw new InvalidOperationException(T("No property named ") + commandSwitch +
                                                            T(" found of type bool, int or string"));
                    }
                }
            }

            foreach (MethodInfo methodInfo in GetType().GetMethods()) {
                if (String.Equals(methodInfo.Name, context.Command, StringComparison.OrdinalIgnoreCase)) {
                    CheckMethodForSwitches(methodInfo, context.Switches);
                    object[] invokeParameters = GetInvokeParametersForMethod(methodInfo, context.Arguments ?? new string[]{});
                    if (invokeParameters != null) {
                        context.Output = (string) methodInfo.Invoke(this, invokeParameters);
                        return;
                    }
                }

                foreach (OrchardCommandAttribute commandAttribute in methodInfo.GetCustomAttributes(typeof(OrchardCommandAttribute), false)) {
                    if (String.Equals(commandAttribute.Command, context.Command, StringComparison.OrdinalIgnoreCase)) {
                        CheckMethodForSwitches(methodInfo, context.Switches);
                        object[] invokeParameters = GetInvokeParametersForMethod(methodInfo, context.Arguments ?? new string[]{});
                        if (invokeParameters != null) {
                            context.Output = (string) methodInfo.Invoke(this, invokeParameters);
                            return;
                        }
                    }
                }
            }

            throw new InvalidOperationException(T("Command : ") + context.Command + T(" was not found "));
        }

        private static object[] GetInvokeParametersForMethod(MethodInfo methodInfo, string[] arguments) {
            List<object> invokeParameters = new List<object>();
            
            List<string> args = new List<string>(arguments);
            ParameterInfo[] methodParameters = methodInfo.GetParameters();
            bool methodHasParams = false;

            if (methodParameters.Length == 0) {
                if (args.Count == 0) return invokeParameters.ToArray();
                return null;
            }

            if (methodParameters[methodParameters.Length - 1].ParameterType.IsAssignableFrom(typeof(string[]))) {
                methodHasParams = true;
            }

            if (!methodHasParams && args.Count != methodParameters.Length) return null;
            if (methodHasParams && (methodParameters.Length - args.Count >= 2)) return null;

            for (int i = 0; i < args.Count; i++) {
                if (methodParameters[i].ParameterType.IsAssignableFrom(typeof(string[]))) {
                    invokeParameters.Add(args.GetRange(i, args.Count - i).ToArray());
                    break;
                }
                invokeParameters.Add(arguments[i]);
            }

            if (methodHasParams && (methodParameters.Length - args.Count == 1)) invokeParameters.Add(new string[] { });

            return invokeParameters.ToArray();
        }

        private void CheckMethodForSwitches(MethodInfo methodInfo, NameValueCollection switches) {
            if (switches == null || switches.AllKeys.Length == 0) 
                return;
            List<string> supportedSwitches = new List<string>();
            foreach (OrchardSwitchesAttribute switchesAttribute in methodInfo.GetCustomAttributes(typeof(OrchardSwitchesAttribute), false)) {
                supportedSwitches.AddRange(switchesAttribute.SwitchName);
            }
            foreach (var commandSwitch in switches.AllKeys) {
                if (!supportedSwitches.Contains(commandSwitch)) {
                    throw new InvalidOperationException(T("Method ") + methodInfo.Name +
                                    T(" does not support switch ") + commandSwitch);
                }
            }
        }

        #endregion
    }
}
