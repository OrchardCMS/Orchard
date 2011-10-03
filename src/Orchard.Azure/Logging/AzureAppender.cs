using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics.Management;
using Microsoft.WindowsAzure.ServiceRuntime;
using log4net.Appender;
using log4net.Core;

namespace Orchard.Azure.Logging {

    /// <summary>
    /// Azure specific implementation of a log4net appender.
    /// Uses DataConnectionString to define the location of the log.
    /// </summary>
    public class AzureAppender : AppenderSkeleton {
        private const string WadConnectionString = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";
        
        public AzureAppender() {
            
            var cloudStorageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue(WadConnectionString));

            var roleInstanceDiagnosticManager = cloudStorageAccount.CreateRoleInstanceDiagnosticManager(
                RoleEnvironment.DeploymentId,
                RoleEnvironment.CurrentRoleInstance.Role.Name,
                RoleEnvironment.CurrentRoleInstance.Id);
    
            var diagnosticMonitorConfiguration = roleInstanceDiagnosticManager.GetCurrentConfiguration();
            diagnosticMonitorConfiguration.Directories.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);
            diagnosticMonitorConfiguration.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(1d);

            roleInstanceDiagnosticManager.SetCurrentConfiguration(diagnosticMonitorConfiguration);
        } 

        protected override void Append(LoggingEvent loggingEvent) {
            var formattedLog = RenderLoggingEvent(loggingEvent);
            
            // this is the way to logging into Azure
            System.Diagnostics.Trace.WriteLine(formattedLog);
        }
    }
}
