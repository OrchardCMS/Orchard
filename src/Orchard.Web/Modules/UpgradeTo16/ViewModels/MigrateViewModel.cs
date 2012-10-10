using System.Collections.Generic;

namespace UpgradeTo16.ViewModels {
    public class MigrateViewModel {
        public IList<ContentTypeEntry> ContentTypes { get; set; }
    }

    public class ContentTypeEntry {
        public string ContentTypeName { get; set; }
        public bool IsChecked { get; set; }
    }
}
