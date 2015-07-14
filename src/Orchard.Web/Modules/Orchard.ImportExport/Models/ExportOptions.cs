using System.Collections.Generic;

namespace Orchard.ImportExport.Models {
    public class ExportOptions {
        public IEnumerable<string> CustomSteps { get; set; }
    }

    public enum VersionHistoryOptions {
        Published,
        Draft,
        Latest
    }
}