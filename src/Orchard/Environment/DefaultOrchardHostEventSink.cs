using System;
using System.Collections.Generic;
using Orchard.Environment.Configuration;
using Orchard.Environment.Topology;
using Orchard.Environment.Topology.Models;
using Orchard.Events;

namespace Orchard.Environment {
    /// <summary>
    /// This handler forwards calls to the IOrchardHost when it is an instance of DefaultOrchardHost.
    /// The reason for this is to avoid adding IEventBusHandler, because DefaultOrchardHost is a component
    /// that should not be detected and registererd automatically as an IDependency.
    /// </summary>
    public class DefaultOrchardHostEventSink : IShellSettingsEventHandler, IShellDescriptorManagerEventHandler {
        private readonly IShellSettingsEventHandler _shellSettingsEventHandler;
        private readonly IShellDescriptorManagerEventHandler _shellDescriptorManagerEventHandler;

        public DefaultOrchardHostEventSink(IOrchardHost host) {
            _shellSettingsEventHandler = host as IShellSettingsEventHandler;
            _shellDescriptorManagerEventHandler = host as IShellDescriptorManagerEventHandler;
        }

        void IShellSettingsEventHandler.Saved(ShellSettings settings) {
            if (_shellSettingsEventHandler != null)
                _shellSettingsEventHandler.Saved(settings);
        }

        void IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor) {
            if (_shellDescriptorManagerEventHandler != null)
                _shellDescriptorManagerEventHandler.Changed(descriptor);
        }
    }
}
