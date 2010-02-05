using Autofac;
using Orchard.Environment.Configuration;

namespace Orchard.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        IContainer CreateContainer(IShellSettings settings);
    }
}