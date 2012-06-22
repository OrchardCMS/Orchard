using System.Collections.Generic;

namespace Orchard.ImportExport.ViewModels {
    public class ExportViewModel {
        public IList<ContentTypeEntry> ContentTypes { get; set; }
        public IList<CustomStepEntry> CustomSteps { get; set; }
        public virtual bool Metadata { get; set; }
        public virtual bool Data { get; set; }
        public virtual string DataImportChoice { get; set; }
        public virtual bool SiteSettings { get; set; }
    }

    public class ContentTypeEntry {
        public string ContentTypeName { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CustomStepEntry {
        public string CustomStep { get; set; }
        public bool IsChecked { get; set; }
    }
}
