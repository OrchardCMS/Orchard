using System;
using Microsoft.WindowsAzure.Diagnostics;
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
            
            var defaultDiagnostics = DiagnosticMonitor.GetDefaultInitialConfiguration();
            var period = TimeSpan.FromMinutes(1d);
            
            defaultDiagnostics.Directories.ScheduledTransferPeriod = period;
            defaultDiagnostics.Logs.ScheduledTransferPeriod = period;
            defaultDiagnostics.WindowsEventLog.ScheduledTransferPeriod = period;

            DiagnosticMonitor.Start(WadConnectionString, defaultDiagnostics);
        } 

        protected override void Append(LoggingEvent loggingEvent) {
            var formattedLog = RenderLoggingEvent(loggingEvent);
            
            // this is the way to logging into Azure
            System.Diagnostics.Trace.WriteLine(formattedLog);
        }
    }
}
