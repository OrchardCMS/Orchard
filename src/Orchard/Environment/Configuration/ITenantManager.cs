using System.Collections.Generic;

namespace Orchard.Environment.Configuration {
    public interface ITenantManager {
        IEnumerable<IShellSettings> LoadSettings();
        void SaveSettings(IShellSettings settings);
    }
}