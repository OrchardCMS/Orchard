using System.Collections.Generic;

namespace Orchard.ImportExport.Models {
    public class ExportOptions {
        public bool ExportMetadata { get; set; }
        public bool ExportData { get; set; }
        public int? ImportBatchSize { get; set; }
        public VersionHistoryOptions VersionHistoryOptions { get; set; }
        public bool ExportSiteSettings { get; set; }
        public IEnumerable<string> CustomSteps { get; set; }
        public bool SetupRecipe { get; set; }
        public string RecipeDescription { get; set; }
        public string RecipeWebsite { get; set; }
        public string RecipeTags { get; set; }
        public string RecipeVersion { get; set; }
    }

    public enum VersionHistoryOptions {
        Published,
        Draft,
    }
}