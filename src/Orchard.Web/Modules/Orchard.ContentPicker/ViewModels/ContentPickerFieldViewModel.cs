using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentPicker.Fields;

namespace Orchard.ContentPicker.ViewModels {

    public class ContentPickerFieldViewModel {

        public ICollection<ContentItem> ContentItems { get; set; }
        public string SelectedIds { get; set; }
        public ContentPickerField Field { get; set; }
        public ContentPart Part { get; set; }
    }
}