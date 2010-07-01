using System;
using Orchard.Caching;

namespace Orchard.Environment.Extensions {
    public class SetupExtensionsContext {
        public bool RestartAppDomain { get; set; }
    }

    public interface IExtensionLoaderCoordinator {
        void SetupExtensions(SetupExtensionsContext setupExtensionsContext);
        void MonitorExtensions(Action<IVolatileToken> monitor);
    }
}