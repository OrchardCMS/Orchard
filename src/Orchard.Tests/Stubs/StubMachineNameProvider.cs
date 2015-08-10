using Orchard.Environment;

namespace Orchard.Tests.Stubs {
    public class StubMachineNameProvider : IMachineNameProvider {
        public string GetMachineName() {
            return "Orchard Machine";
        }
    }
}
