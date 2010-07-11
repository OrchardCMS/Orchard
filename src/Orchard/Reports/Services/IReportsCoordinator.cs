using Orchard.Localization;

namespace Orchard.Reports.Services {
    public interface IReportsCoordinator : IDependency {
        void Add(string reportKey, ReportEntryType type, string message);
        void Register(string reportKey, string activityName, string title);
    }
}
