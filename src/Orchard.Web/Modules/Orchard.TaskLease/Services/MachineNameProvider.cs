namespace Orchard.TaskLease.Services
{
    public class MachineNameProvider : IMachineNameProvider
    {
        public string GetMachineName()
        {
            return System.Environment.MachineName;
        }
    }
}