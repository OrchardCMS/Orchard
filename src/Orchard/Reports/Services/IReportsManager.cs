using System.Collections.Generic;

namespace Orchard.Reports.Services {
    public interface IReportsManager : ISingletonDependency {
        void Add(int reportId, ReportEntryType type, string message);
        int CreateReport(string title, string activityName);
        Report Get(int reportId);
        IEnumerable<Report> GetReports();
        void Flush();
    }
}
