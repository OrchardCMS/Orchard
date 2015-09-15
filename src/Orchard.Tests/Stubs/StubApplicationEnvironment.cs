using Orchard.Environment;

namespace Orchard.Tests.Stubs {
    public class StubApplicationEnvironment : IApplicationEnvironment {
        public StubApplicationEnvironment() {
            MachineName = "Orchard Machine";
        }
        public string MachineName { get; set; }
        public string GetEnvironmentIdentifier() {
            return MachineName;
        }
    }
}
