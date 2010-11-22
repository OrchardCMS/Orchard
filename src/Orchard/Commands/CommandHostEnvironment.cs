using Orchard.Environment;
using Orchard.Localization;

namespace Orchard.Commands {
    internal class CommandHostEnvironment : HostEnvironment {
        public CommandHostEnvironment() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void ResetSiteCompilation() {
            throw new OrchardCommandHostRetryException(T("A change of configuration requires the session to be restarted."));
        }
    }
}