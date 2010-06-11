using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Orchard.Azure.Web {
    public class WebRole : RoleEntryPoint {
        public override bool OnStart() {
            DiagnosticMonitor.Start("DiagnosticsConnectionString");

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;

            return base.OnStart();
        }

        private void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e) {
            // If a configuration setting is changing
            if ( e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange) ) {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }
    }
}
