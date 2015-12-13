using log4net.Appender;
using log4net.Core;
using System.Diagnostics;

namespace Orchard.Azure.Services.Logging {

    /// <summary>
    /// Provides a Log4net appender implementation that sends log messages to Microsoft Azure Diagnostics.
    /// </summary>
    public class AzureDiagnosticsAppender : AppenderSkeleton {

        protected override void Append(LoggingEvent loggingEvent) {
            var formattedMessage = RenderLoggingEvent(loggingEvent);
            if (loggingEvent.Level == Level.Info)
                Trace.TraceInformation(formattedMessage);
            if (loggingEvent.Level == Level.Warn)
                Trace.TraceWarning(formattedMessage);
            if (loggingEvent.Level == Level.Error || loggingEvent.Level == Level.Fatal)
                Trace.TraceError(formattedMessage);
            else
                Trace.WriteLine(formattedMessage);
        }
    }
}
