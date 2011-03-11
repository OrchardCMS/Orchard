namespace Orchard.ImportExport.Models {
    public class DataExportOptions {
        public bool ExportData { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }
    }

    public enum VersionHistoryOptions {
        Published,
        Draft,
        AllVersions
    }
}