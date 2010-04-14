using System;
using System.Collections.Generic;

namespace Orchard.Environment.Configuration {
    public class AzureBlobTenantManager : ITenantManager{
        public AzureBlobTenantManager(string foo) {
            int x = 5;
        }

        public string Foo { get; set; }

        public IEnumerable<ShellSettings> LoadSettings() {
            throw new NotImplementedException();
        }

        public void SaveSettings(ShellSettings settings) {
            throw new NotImplementedException();
        }
    }
}
