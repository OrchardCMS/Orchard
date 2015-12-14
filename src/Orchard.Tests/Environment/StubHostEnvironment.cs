using Orchard.Environment;

namespace Orchard.Tests.Environment {
    public class StubHostEnvironment : HostEnvironment {
        public override void RestartAppDomain() {
        }
    }
}
