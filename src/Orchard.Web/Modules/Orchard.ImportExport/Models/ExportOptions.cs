using System.Collections.Generic;

namespace Orchard.ImportExport.Models {
    public class ExportOptions {
        public bool ExportMetadata { get; set; }
        public bool ExportData { get; set; }
        public bool ExportFiles { get; set; }
        public bool ExportAsDraft { get; set; }
        public int? ImportBatchSize { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }
        public bool ExportSiteSettings { get; set; }
        public IEnumerable<string> CustomSteps { get; set; }
    }

    public enum VersionHistoryOptions {
        Published,
        Draft,
    }
}