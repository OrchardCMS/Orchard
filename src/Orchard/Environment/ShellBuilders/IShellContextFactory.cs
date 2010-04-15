using Orchard.Environment.Configuration;

namespace Orchard.Environment.ShellBuilders {
    public interface IShellContextFactory {
        ShellContext Create(ShellSettings settings);
    }
}
