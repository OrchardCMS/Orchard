using System;

namespace Orchard.Environment.Warmup {
    public class StartupResult {
        public IOrchardHost Host { get; set; }
        public Exception Error { get; set; }
    }
}
