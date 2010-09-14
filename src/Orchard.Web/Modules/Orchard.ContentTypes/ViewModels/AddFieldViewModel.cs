using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentTypes.ViewModels {
    public class AddFieldViewModel {
        public AddFieldViewModel() {
            Fields = new List<ContentFieldInfo>();
        }

        public string DisplayName { get; set; }
        public string FieldTypeName { get; set; }
        public EditPartViewModel Part { get; set; }
        public IEnumerable<ContentFieldInfo> Fields { get; set; }
    }
}