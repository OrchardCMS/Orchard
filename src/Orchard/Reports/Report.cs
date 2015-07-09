using System;
using System.Collections.Generic;

namespace Orchard.Reports {
    public class Report {
        public Report() {
            Entries = new List<ReportEntry>();
        }

        public IList<ReportEntry> Entries { get; set;}
        public int ReportId { get; set; }
        public string Title { get; set; }
        public string ActivityName { get; set; }
        public DateTime Utc { get; set; }
    }
}
