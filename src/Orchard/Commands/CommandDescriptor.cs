using System;
using System.Reflection;

namespace Orchard.Commands {
    public class CommandDescriptor {
        public string Name { get; set; }
        public MethodInfo MethodInfo { get; set; }
    }
}