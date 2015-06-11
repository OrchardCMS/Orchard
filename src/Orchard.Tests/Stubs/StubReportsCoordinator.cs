using Orchard.Reports;
using Orchard.Reports.Services;

namespace Orchard.Tests.Stubs {
    public class StubReportsCoordinator : IReportsCoordinator {
        public void Add(string reportKey, ReportEntryType type, string message) {
            
        }

        public int Register(string reportKey, string activityName, string title) {
            return 0;
        }
    }
}