using Orchard.Environment;

namespace Orchard.Specs.Hosting {
    public class SpecHostEnvironment : HostEnvironment {
        public SpecHostEnvironment() {
        }

        public override void RestartAppDomain() {
            // do nothing
        }
    }
}