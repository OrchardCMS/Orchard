using System;
using Microsoft.WindowsAzure.Diagnostics;
using log4net.Appender;
using log4net.Core;

namespace Orchard.Azure.Services.Logging {

    /// <summary>
    /// Provides a Log4net appender implementation that sends log messages to Windows Azure Diagnostics.
    /// </summary>
    public class AzureDiagnosticsAppender : AppenderSkeleton {

        private const string _wadStorageAccountConnectionStringSettingName = "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString";
        
        public AzureDiagnosticsAppender() {
            var defaultDiagnostics = DiagnosticMonitor.GetDefaultInitialConfiguration();
            var period = TimeSpan.FromMinutes(1d);

            defaultDiagnostics.Directories.ScheduledTransferPeriod = period;
            defaultDiagnostics.Logs.ScheduledTransferPeriod = period;
            defaultDiagnostics.WindowsEventLog.ScheduledTransferPeriod = period;

            DiagnosticMonitor.Start(_wadStorageAccountConnectionStringSettingName, defaultDiagnostics);
        }

        protected override void Append(LoggingEvent loggingEvent) {
            var formattedMessage = RenderLoggingEvent(loggingEvent);
            System.Diagnostics.Trace.WriteLine(formattedMessage);
        }
    }
}
