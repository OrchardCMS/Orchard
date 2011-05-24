using System;
using System.Collections;
using Autofac.Core;

namespace Orchard.Environment {
    internal class CollectionOrderModule : IModule {
        public void Configure(IComponentRegistry componentRegistry) {
            componentRegistry.Registered += (s, e) => {
                // only bother watching enumerable resolves
                var limitType = e.ComponentRegistration.Activator.LimitType;
                if (typeof(IEnumerable).IsAssignableFrom(limitType)) {
                    e.ComponentRegistration.Activated += (s2, e2) => {
                        // Autofac's IEnumerable feature returns an Array
                        if (e2.Instance is Array) {
                            // Orchard needs FIFO, not FILO, component order
                            Array.Reverse((Array)e2.Instance);
                        }
                    };
                }
            };
        }
    }
}
