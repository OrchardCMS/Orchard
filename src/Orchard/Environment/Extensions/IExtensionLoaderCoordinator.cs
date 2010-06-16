using System;
using Orchard.Caching;

namespace Orchard.Environment.Extensions {
    public interface IExtensionLoaderCoordinator {
        void SetupExtensions();
        void MonitorExtensions(Action<IVolatileToken> monitor);
    }
}