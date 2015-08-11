using Orchard.Environment;

namespace Orchard.Tests.Stubs {
    public class StubMachineNameProvider : IMachineNameProvider {
        public StubMachineNameProvider() {
            MachineName = "Orchard Machine";
        }
        public string MachineName { get; set; }
        public string GetMachineName() {
            return MachineName;
        }
    }
}
