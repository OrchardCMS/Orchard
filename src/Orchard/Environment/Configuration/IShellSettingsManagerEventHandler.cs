using Orchard.Events;

namespace Orchard.Environment.Configuration {
    public interface IShellSettingsManagerEventHandler : IEventHandler {
        void Saved(ShellSettings settings);
    }
}