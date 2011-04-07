namespace Orchard.ImportExport.Models {
    public class ExportOptions {
        public bool ExportMetadata { get; set; }
        public bool ExportData { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }
        public bool ExportSiteSettings { get; set; }
    }

    public enum VersionHistoryOptions {
        Published,
        Draft,
    }
}