namespace Orchard.Reports.Services {
    /// <summary>
    /// Exposes a simplified interface for creating reports. Reports provide user-accessible log-like functionality.
    /// </summary>
    /// <remarks>
    /// <see cref="Orchard.Reports.Services.IReportsManager"/> can be used too to create reports directly.
    /// </remarks>
    public interface IReportsCoordinator : IDependency {
        /// <summary>
        /// Adds a new report entry to a report that was previously registered.
        /// </summary>
        /// <remarks>
        /// Entries can be only added to a report that was previously registered through Register().
        /// </remarks>
        /// <seealso cref="Register()"/>
        /// <param name="reportKey">Key, i.e. technical name of the report. Should be the same as the one used when registering the report.</param>
        /// <param name="type">Type of the entry.</param>
        /// <param name="message">The message to include in the entry.</param>
        void Add(string reportKey, ReportEntryType type, string message);

        /// <summary>
        /// Registers a new report so entries can be added to it.
        /// </summary>
        /// <param name="reportKey">Key, i.e. technical name of the report.</param>
        /// <param name="activityName">Name of the activity the report is about (e.g. "Upgrade").</param>
        /// <param name="title">A title better describing what the report is about (e.g. "Migrating routes of Pages, Blog Posts").</param>
        /// <returns>The report's numerical ID.</returns>
        int Register(string reportKey, string activityName, string title);
    }
}
