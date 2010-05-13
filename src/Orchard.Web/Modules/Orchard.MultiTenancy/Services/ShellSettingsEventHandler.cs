using System;
using Orchard.Events;
using Orchard.Environment.Configuration;
using Orchard.Environment;

namespace Orchard.MultiTenancy.Services {
    public class ShellSettingsEventHandler : IShellSettingsEventHandler, IDependency {

        private readonly IOrchardHost _orchardHost;

        public ShellSettingsEventHandler(IOrchardHost orchardHost) {
            _orchardHost = orchardHost;        
        }

        public void Saved(ShellSettings settings) {
            _orchardHost.InvalidateShells();
        }
    }
}
