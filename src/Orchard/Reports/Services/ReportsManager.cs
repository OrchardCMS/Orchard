using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Logging;

namespace Orchard.Reports.Services {
    public class ReportsManager : IReportsManager {
        private readonly IReportsPersister _reportsPersister;
        private List<Report> _reports;
        private static readonly object _synLock = new object();
        private bool _isDirty;

        public ReportsManager(IReportsPersister reportsPersister) {
            _reportsPersister = reportsPersister;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Add(int reportId, ReportEntryType type, string message) {
            lock ( _synLock ) {
                LoadReports();
                _isDirty = true;
                var report = Get(reportId);
                if(report == null) {
                    return;
                }
                report.Entries.Add(new ReportEntry {Message = message, Type = type, Utc = DateTime.UtcNow});
            }
        }

        public int CreateReport(string title, string activityName) {
            lock ( _synLock ) {
                LoadReports();
                _isDirty = true;
                var reportId = _reports.Count == 0 ? 1 : _reports.Max(r => r.ReportId) + 1;
                var report = new Report {ActivityName = activityName, ReportId = reportId, Title = title, Utc = DateTime.UtcNow};
                _reports.Add(report);
                return reportId;
            }
        }

        public Report Get(int reportId) {
            lock(_synLock) {
                LoadReports();
                return _reports.Where(r => r.ReportId == reportId).FirstOrDefault();
            }
        }

        public IEnumerable<Report> GetReports() {
            lock ( _synLock ) {
                LoadReports();
                return _reports.ToList();
            }
        }

        public void Flush() {
            if ( _reports == null || !_isDirty) {
                return;
            }

            lock ( _synLock ) {
                _reportsPersister.Save(_reports);
                _isDirty = false;
            }
        }

        private void LoadReports() {
            if(_reports == null) {
                _reports = _reportsPersister.Fetch().ToList();
            }
        }
    }
}
