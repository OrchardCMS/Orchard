﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orchard.Commands {
    public class CommandHandlerDescriptorBuilder {
        public CommandHandlerDescriptor Build(Type type) {
            return new CommandHandlerDescriptor { Commands = CollectMethods(type) };
        }

        private IEnumerable<CommandDescriptor> CollectMethods(Type type) {
            var methods = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

            foreach (var methodInfo in methods) {
                yield return BuildMethod(methodInfo);
            }
        }

        private CommandDescriptor BuildMethod(MethodInfo methodInfo) {
            return new CommandDescriptor {
                                             Name = GetCommandName(methodInfo),
                                             MethodInfo = methodInfo,
                                             HelpText = GetCommandHelpText(methodInfo)
                                         };
        }

        private string GetCommandHelpText(MethodInfo methodInfo) {
            var attributes = methodInfo.GetCustomAttributes(typeof(CommandHelpAttribute), false/*inherit*/);
            if (attributes != null && attributes.Any()) {
                return attributes.Cast<CommandHelpAttribute>().Single().HelpText;
            }
            return string.Empty;
        }

        private string GetCommandName(MethodInfo methodInfo) {
            var attributes = methodInfo.GetCustomAttributes(typeof(CommandNameAttribute), false/*inherit*/);
            if (attributes != null && attributes.Any()) {
                return attributes.Cast<CommandNameAttribute>().Single().Command;
            }

            return methodInfo.Name.Replace('_', ' ');
        }
    }
}
