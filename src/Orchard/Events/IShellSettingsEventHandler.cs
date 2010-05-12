using Orchard.Environment.Configuration;

namespace Orchard.Events {
    public interface IShellSettingsEventHandler : IEventHandler {
        void Created(ShellSettings settings);
        void Deleted(ShellSettings settings);
        void Updated(ShellSettings settings);
    }
}
