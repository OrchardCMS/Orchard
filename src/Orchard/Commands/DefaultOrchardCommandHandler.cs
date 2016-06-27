﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Orchard.Localization;
using Orchard.Exceptions;

namespace Orchard.Commands {
    public abstract class DefaultOrchardCommandHandler : ICommandHandler {
        protected DefaultOrchardCommandHandler() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public CommandContext Context { get; set; }

        public void Execute(CommandContext context) {
            SetSwitchValues(context);
            Invoke(context);
        }

        private void SetSwitchValues(CommandContext context) {
            if (context.Switches != null && context.Switches.Any()) {
                foreach (var commandSwitch in context.Switches) {
                    SetSwitchValue(commandSwitch);
                }
            }
        }

        private void SetSwitchValue(KeyValuePair<string, string> commandSwitch) {
            // Find the property
            PropertyInfo propertyInfo = GetType().GetProperty(commandSwitch.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (propertyInfo == null) {
                throw new InvalidOperationException(T("Switch \"{0}\" was not found", commandSwitch.Key).Text);
            }
            if (propertyInfo.GetCustomAttributes(typeof(OrchardSwitchAttribute), false).Length == 0) {
                throw new InvalidOperationException(T("A property \"{0}\" exists but is not decorated with \"{1}\"", commandSwitch.Key, typeof(OrchardSwitchAttribute).Name).Text);
            }

            // Set the value
            try {
                object value = Convert.ChangeType(commandSwitch.Value, propertyInfo.PropertyType);
                propertyInfo.SetValue(this, value, null/*index*/);
            }
            catch(Exception ex) {
                if (ex.IsFatal()) {
                    throw;
                } 
                string message = T("Error converting value \"{0}\" to \"{1}\" for switch \"{2}\"",
                    LocalizedString.TextOrDefault(commandSwitch.Value, T("(empty)")), 
                    propertyInfo.PropertyType.FullName, 
                    commandSwitch.Key).Text;
                throw new InvalidOperationException(message, ex);
            }
        }

        private void Invoke(CommandContext context) {
            CheckMethodForSwitches(context.CommandDescriptor.MethodInfo, context.Switches);

            var arguments = (context.Arguments ?? Enumerable.Empty<string>()).ToArray();
            object[] invokeParameters = GetInvokeParametersForMethod(context.CommandDescriptor.MethodInfo, arguments);
            if (invokeParameters == null) {
                throw new InvalidOperationException(T("Command arguments \"{0}\" don't match command definition", string.Join(" ", arguments)).ToString());
            }

            this.Context = context;
            var result = context.CommandDescriptor.MethodInfo.Invoke(this, invokeParameters);
            if (result is string)
                context.Output.Write(result);
        }

        private static object[] GetInvokeParametersForMethod(MethodInfo methodInfo, IList<string> arguments) {
            var invokeParameters = new List<object>();
            var args = new List<string>(arguments);
            var methodParameters = methodInfo.GetParameters();
            bool methodHasParams = false;

            if (methodParameters.Length == 0) {
                if (args.Count == 0)
                    return invokeParameters.ToArray();
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
                invokeParameters.Add(Convert.ChangeType(arguments[i], methodParameters[i].ParameterType));
            }

            if (methodHasParams && (methodParameters.Length - args.Count == 1)) invokeParameters.Add(new string[] { });

            return invokeParameters.ToArray();
        }

        private void CheckMethodForSwitches(MethodInfo methodInfo, IDictionary<string, string> switches) {
            if (switches == null || switches.Count == 0)
                return;

            var supportedSwitches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (OrchardSwitchesAttribute switchesAttribute in methodInfo.GetCustomAttributes(typeof(OrchardSwitchesAttribute), false)) {
                supportedSwitches.UnionWith(switchesAttribute.Switches);
            }

            foreach (var commandSwitch in switches.Keys) {
                if (!supportedSwitches.Contains(commandSwitch)) {
                    throw new InvalidOperationException(T("Method \"{0}\" does not support switch \"{1}\".", methodInfo.Name, commandSwitch).ToString());
                }
            }
        }
    }
}
