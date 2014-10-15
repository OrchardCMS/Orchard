using Orchard.Layouts.Framework.Drivers;
using Orchard.Localization;

namespace Orchard.Layouts.ViewModels {
    public class ElementWrapperPartViewModel {
        public EditorResult ElementEditorResult { get; set; }
        public dynamic ElementEditor { get; set; }
        public string ElementTypeName { get; set; }
        public LocalizedString ElementDisplayText { get; set; }
    }
}