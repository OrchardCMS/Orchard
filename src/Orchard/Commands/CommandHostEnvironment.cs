using System.Reflection;
using Orchard.Environment;
using Orchard.Localization;

namespace Orchard.Commands {
    internal class CommandHostEnvironment : HostEnvironment {
        public CommandHostEnvironment() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override void RestartAppDomain() {
            throw new OrchardCommandHostRetryException(T("A change of configuration requires the session to be restarted."));
        }
    }
}