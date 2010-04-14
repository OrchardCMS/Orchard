using Autofac;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology.Models;

namespace Orchard.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        ILifetimeScope CreateContainer(ShellTopology topology);
    }

    public interface IShellContainerFactory_Obsolete {
        ILifetimeScope CreateContainer(ShellSettings settings);
    }
}