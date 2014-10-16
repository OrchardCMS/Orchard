using System.Collections.Generic;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Localization;

namespace Orchard.Layouts.ViewModels {
    public class ElementWrapperPartViewModel {
        public IList<string> Tabs { get; set; }
        public EditorResult ElementEditorResult { get; set; }
        public IList<dynamic> ElementEditors { get; set; }
        public string ElementTypeName { get; set; }
        public LocalizedString ElementDisplayText { get; set; }
    }
}