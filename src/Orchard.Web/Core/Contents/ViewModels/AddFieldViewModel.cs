using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class AddFieldViewModel : BaseViewModel {
        public AddFieldViewModel() {
            Fields = new List<ContentFieldInfo>();
        }

        public string DisplayName { get; set; }
        public string FieldTypeName { get; set; }
        public EditPartViewModel Part { get; set; }
        public IEnumerable<ContentFieldInfo> Fields { get; set; }
    }
}