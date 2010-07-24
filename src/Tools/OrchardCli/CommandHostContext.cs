using System;
using System.IO;
using System.Web.Hosting;
using Orchard;
using Orchard.Host;

namespace OrchardCLI {
    public class CommandHostContext {
        public int StartSessionResult { get; set; }
        public int RetryResult { get; set; }
        public OrchardParameters Arguments { get; set; }
        public DirectoryInfo OrchardDirectory { get; set; }
        public ApplicationManager AppManager { get; set; }
        public ApplicationObject AppObject { get; set; }
        public CommandHost CommandHost { get; set; }
        public Logger Logger { get; set; }
    }
}