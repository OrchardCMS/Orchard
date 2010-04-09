using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orchard.Commands {
    public class CommandHandlerDescriptorBuilder {
        public CommandHandlerDescriptor Build(Type type) {
            var methods = CollectMethods(type);
            return new CommandHandlerDescriptor { Commands = methods };
        }

        private IEnumerable<CommandDescriptor> CollectMethods(Type type) {
            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)) {
                var name = GetCommandName(methodInfo);
                yield return new CommandDescriptor { Name = name, MethodInfo = methodInfo };
            }
        }

        private string GetCommandName(MethodInfo methodInfo) {
            var attributes = methodInfo.GetCustomAttributes(typeof(OrchardCommandAttribute), false/*inherit*/);
            if (attributes != null && attributes.Any()) {
                return attributes.Cast<OrchardCommandAttribute>().Single().Command;
            }

            return methodInfo.Name.Replace('_', ' ');
        }
    }
}
