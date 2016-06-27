using Orchard.Environment.Extensions;
using Orchard.Security;

namespace Orchard.SecureSocketsLayer.Services {
    [OrchardSuppressDependency("Orchard.Security.Providers.DefaultSslSettingsProvider")]
    public class SecureSocketsLayerSettingsProvider : ISslSettingsProvider {
        private readonly ISecureSocketsLayerService _secureSocketsLayerService;

        public SecureSocketsLayerSettingsProvider(ISecureSocketsLayerService secureSocketsLayerService) {
            _secureSocketsLayerService = secureSocketsLayerService;
        }

        public bool GetRequiresSSL() {
            return _secureSocketsLayerService.GetSettings().Enabled;
        }
    }
}
