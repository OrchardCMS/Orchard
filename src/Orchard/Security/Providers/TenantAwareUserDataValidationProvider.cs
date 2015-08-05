using System;
using Orchard.Environment.Configuration;

namespace Orchard.Security.Providers {
    public class TenantAwareUserDataValidationProvider : IUserDataValidationProvider {
        private readonly ShellSettings _shellSettings;

        public TenantAwareUserDataValidationProvider(ShellSettings shellSettings) {
            _shellSettings = shellSettings;
        }

        public string Key { get { return "Tenant"; } }

        public string GetUserData() {
            return _shellSettings.Name;
        }

        public bool ValidateUserData(string value) {
            return String.Equals(value, _shellSettings.Name, StringComparison.Ordinal);
        }
    }
}