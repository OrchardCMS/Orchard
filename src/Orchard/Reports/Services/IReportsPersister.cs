using System.Collections.Generic;

namespace Orchard.Reports.Services {
    /// <summary>
    /// Defines a service that can be used to persist reports.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are commonly used from <see cref="Orchard.Reports.Services.IReportsManager"/> implementations.
    /// </remarks>
    public interface IReportsPersister : IDependency {
        IEnumerable<Report> Fetch();
        void Save(IEnumerable<Report> reports);
    }
}
