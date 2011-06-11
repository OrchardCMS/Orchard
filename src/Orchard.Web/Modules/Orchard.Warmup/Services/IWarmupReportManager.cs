using System.Collections.Generic;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.Services {
    public interface IWarmupReportManager : IDependency {
        IEnumerable<ReportEntry> Read();
        void Create(IEnumerable<ReportEntry> reportEntries);
    }
}