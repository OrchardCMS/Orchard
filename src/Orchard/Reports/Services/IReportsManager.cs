using System.Collections.Generic;

namespace Orchard.Reports.Services {
    /// <summary>
    /// Service for handling reports. Reports provide user-accessible log-like functionality.
    /// </summary>
    /// <remarks>
    /// You can use <see cref="Orchard.Reports.Services.IReportsCoordinator"/> to create reports through a simplified interface.
    /// </remarks>
    public interface IReportsManager : ISingletonDependency {
        void Add(int reportId, ReportEntryType type, string message);
        int CreateReport(string title, string activityName);
        Report Get(int reportId);
        IEnumerable<Report> GetReports();
        void Flush();
    }
}
