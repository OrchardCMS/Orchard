using System.IO;
using System.Web.Hosting;
using Orchard.Host;

namespace Orchard.HostContext {
    public class CommandHostContext {
        public int RetryResult { get; set; }
        public OrchardParameters Arguments { get; set; }
        public DirectoryInfo OrchardDirectory { get; set; }
        public int StartSessionResult { get; set; }
        public bool DisplayUsageHelp { get; set; }
        public CommandHost CommandHost { get; set; }
        public Logger Logger { get; set; }
    }
}