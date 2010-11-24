using System;

namespace Orchard.Specs.Hosting {
    [Flags]
    public enum ExtensionDeploymentOptions {
        CompiledAssembly = 0x01,
        SourceCode = 0x02,
    }
}