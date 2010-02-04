using Orchard.Localization;
using Orchard.Logging;

namespace Orchard.Core.Setup.Services {
    public class SetupService : ISetupService {
        public SetupService(IOrchardServices services) {
            Services = services;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        private Localizer T { get; set; }
    }
}
