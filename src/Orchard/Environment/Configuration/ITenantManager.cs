using System.Collections.Generic;

namespace Orchard.Environment.Configuration {
    public interface ITenantManager {
        IEnumerable<ShellSettings> LoadSettings();
        void SaveSettings(ShellSettings settings);
    }
}