using Autofac;

namespace Orchard.Environment.ShellBuilders {
    public interface IShellContainerFactory {
        IContainer CreateContainer(IShellSettings settings);
    }
}