using System;

namespace Orchard.Reports {
    public enum ReportEntryType {
        Information,
        Warning,
        Error
    }

    public class ReportEntry {
        public ReportEntryType Type { get; set; }
        public string Message { get; set; }
        public DateTime Utc { get; set; }
    }
}
