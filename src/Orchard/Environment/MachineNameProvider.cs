namespace Orchard.Environment {
    public class MachineNameProvider : IMachineNameProvider {
        public string GetMachineName() {
            return System.Environment.MachineName;
        }
    }
}