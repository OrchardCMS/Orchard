using Orchard.Environment.Configuration;

namespace Orchard.Events {
    public interface IShellSettingsEventHandler : IEventHandler {
        void Saved(ShellSettings settings);
    }
}
