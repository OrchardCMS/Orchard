using Orchard.Reports;
using Orchard.Reports.Services;

public static class ReportExtentions {
    /// <summary>
    /// Adds a new report entry of type information to a report that was previously registered.
    /// </summary>
    /// <seealso cref="Register()"/>
    /// <param name="reportKey">Key, i.e. technical name of the report. Should be the same as the one used when registering the report.</param>
    /// <param name="message">The message to include in the entry.</param>
    public static void Information(this IReportsCoordinator reportCoordinator, string reportKey, string message) {
        reportCoordinator.Add(reportKey, ReportEntryType.Information, message);
    }

    /// <summary>
    /// Adds a new report entry of type warning to a report that was previously registered.
    /// </summary>
    /// <seealso cref="Register()"/>
    /// <param name="reportKey">Key, i.e. technical name of the report. Should be the same as the one used when registering the report.</param>
    /// <param name="message">The message to include in the entry.</param>
    public static void Warning(this IReportsCoordinator reportCoordinator, string reportKey, string message) {
        reportCoordinator.Add(reportKey, ReportEntryType.Warning, message);
    }

    /// <summary>
    /// Adds a new report entry of type error to a report that was previously registered.
    /// </summary>
    /// <seealso cref="Register()"/>
    /// <param name="reportKey">Key, i.e. technical name of the report. Should be the same as the one used when registering the report.</param>
    /// <param name="message">The message to include in the entry.</param>
    public static void Error(this IReportsCoordinator reportCoordinator, string reportKey, string message) {
        reportCoordinator.Add(reportKey, ReportEntryType.Error, message);
    }
}

