using System.Diagnostics;

namespace Orchard.Environment
{
    public class ApplicationEnvironment : IApplicationEnvironment
    {
        public string GetEnvironmentIdentifier()
        {
            // Add process ID to machine name because multiple web servers can
            // be running on the same physical machine.
            return string.Format("{0}:{1}", System.Environment.MachineName, Process.GetCurrentProcess().Id);
        }
    }
}