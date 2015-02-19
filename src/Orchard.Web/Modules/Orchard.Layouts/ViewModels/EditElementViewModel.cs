using System.Collections.Generic;
using Orchard.Layouts.Framework.Drivers;
using Orchard.Layouts.Models;
using Orchard.Localization;

namespace Orchard.Layouts.ViewModels {
    public class EditElementViewModel {
        public EditElementViewModel() {
            Tabs = new List<string>();
        }

        public EditorResult EditorResult { get; set; }
        public string TypeName { get; set; }
        public LocalizedString DisplayText { get; set; }
        public string ElementData { get; set; }
        public string ElementHtml { get; set; }
        public ILayoutAspect Layout { get; set; }
        public IList<string> Tabs { get; set; }
        public bool Submitted { get; set; }
        public string SessionKey { get; set; }
        public object ElementEditorModel { get; set; }
    }
}