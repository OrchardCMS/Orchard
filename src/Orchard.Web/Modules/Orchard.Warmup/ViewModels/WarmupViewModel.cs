using System.Collections.Generic;
using Orchard.Warmup.Models;

namespace Orchard.Warmup.ViewModels {
    public class WarmupViewModel {
        public WarmupSettingsPart Settings { get; set; }
        public IEnumerable<ReportEntry> ReportEntries { get; set; }
    }
}