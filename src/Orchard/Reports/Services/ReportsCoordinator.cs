using System;
using System.Collections.Generic;
using Orchard.Logging;

namespace Orchard.Reports.Services {
    public class ReportsCoordinator : IReportsCoordinator, IDisposable {
        private readonly IReportsManager _reportsManager;
        private readonly IDictionary<string, int> _reports;

        public ReportsCoordinator(IReportsManager reportsManager) {
            _reportsManager = reportsManager;
            Logger = NullLogger.Instance;
            _reports = new Dictionary<string, int>();
        }

        public ILogger Logger { get; set; }
        public void Dispose() {
            _reportsManager.Flush();
        }

        public void Add(string reportKey, ReportEntryType type, string message) {
            if(!_reports.ContainsKey(reportKey)) {
                // ignore message if no corresponding report
                return;
            }

            _reportsManager.Add(_reports[reportKey], type, message);
        }

        public int Register(string reportKey, string activityName, string title) {
            int reportId = _reportsManager.CreateReport(title, activityName);
            _reports.Add(reportKey, reportId);
            return reportId;
        }
    }
}
