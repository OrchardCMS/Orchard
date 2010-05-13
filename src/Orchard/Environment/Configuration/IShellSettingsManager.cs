using System.Collections.Generic;

namespace Orchard.Environment.Configuration {
    public interface IShellSettingsManager {
        IEnumerable<ShellSettings> LoadSettings();
        void SaveSettings(ShellSettings settings);
    }
}