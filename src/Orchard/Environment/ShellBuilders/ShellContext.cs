using Autofac;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.ShellBuilders {
    public class ShellContext {
        public ShellSettings Settings { get; set; }
        public ShellDescriptor Descriptor { get; set; }
        public ShellTopology Topology { get; set; }
        public ILifetimeScope LifetimeScope { get; set; }
        public IOrchardShell Shell { get; set; }
    }
}