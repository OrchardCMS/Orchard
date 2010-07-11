using System.Collections.Generic;

namespace Orchard.Reports.Services {
    public interface IReportsPersister : IDependency {
        IEnumerable<Report> Fetch();
        void Save(IEnumerable<Report> reports);
    }
}
