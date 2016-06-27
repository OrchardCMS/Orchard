using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Core.Common.Settings;

namespace Orchard.Core.Common.ViewModels {
    public class TextFieldDriverViewModel {
        public TextField Field { get; set; }
        public string Text { get; set; }
        public TextFieldSettings Settings { get; set; }
        public IContent ContentItem { get; set; }
    }
}