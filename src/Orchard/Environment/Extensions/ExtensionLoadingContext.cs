using System;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions {
    public class ExtensionLoadingContext {
        public ExtensionLoadingContext() {
            DeleteActions = new List<Action>();
            CopyActions = new List<Action>();
        }

        public IList<Action> DeleteActions { get; set; }
        public IList<Action> CopyActions { get; set; }

        public bool RestartAppDomain { get; set; }
        public bool ResetSiteCompilation { get; set; }
    }
}