using System;

namespace Orchard.Warmup.Models {
    public class ReportEntry {
        public string RelativeUrl { get; set; }
        public string Filename { get; set; }
        public int StatusCode { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}