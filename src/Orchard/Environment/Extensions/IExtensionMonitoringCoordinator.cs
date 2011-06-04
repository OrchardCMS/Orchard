using System;
using Orchard.Caching;

namespace Orchard.Environment.Extensions {
    public interface IExtensionMonitoringCoordinator {
        void MonitorExtensions(Action<IVolatileToken> monitor);
    }
}