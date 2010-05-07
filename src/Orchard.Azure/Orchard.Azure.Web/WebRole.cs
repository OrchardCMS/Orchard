using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Orchard.Azure.Web {
    public class WebRole : RoleEntryPoint {
        public override bool OnStart() {
            DiagnosticMonitor.Start("DiagnosticsConnectionString");

            #region Setup CloudStorageAccount Configuration Setting Publisher

            // This code sets up a handler to update CloudStorageAccount instances when their corresponding
            // configuration settings change in the service configuration file.
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) => {
                // Provide the configSetter with the initial value
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));

                RoleEnvironment.Changed += (sender, arg) => {
                    if ( arg.Changes.OfType<RoleEnvironmentConfigurationSettingChange>()
                        .Any(change => ( change.ConfigurationSettingName == configName )) ) {
                        // The corresponding configuration setting has changed, propagate the value
                        if ( !configSetter(RoleEnvironment.GetConfigurationSettingValue(configName)) ) {
                            // In this case, the change to the storage account credentials in the
                            // service configuration is significant enough that the role needs to be
                            // recycled in order to use the latest settings. (for example, the 
                            // endpoint has changed)
                            RoleEnvironment.RequestRecycle();
                        }
                    }
                };
            });
            #endregion


            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += (sender, e) => {
                                                // If a configuration setting is changing
                                                if (
                                                    e.Changes.Any(
                                                        change => change is RoleEnvironmentConfigurationSettingChange) ) {
                                                    // Set e.Cancel to true to restart this role instance
                                                    e.Cancel = true;
                                                }
                                            };

            return base.OnStart();
        }

    }
}
