using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.MediaLibrary.Fields;

namespace Orchard.MediaLibrary.ViewModels {

    public class MediaLibraryPickerFieldViewModel {

        public ICollection<ContentItem> ContentItems { get; set; }
        public string SelectedIds { get; set; }
        public MediaLibraryPickerField Field { get; set; }
        public ContentPart Part { get; set; }
    }
}