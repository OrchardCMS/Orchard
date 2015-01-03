using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.ViewModels {
    public class LayoutEditorViewModel {
        public string State { get; set; }
        public dynamic LayoutRoot { get; set; }
        public IList<LayoutPart> Templates { get; set; }
        public int? SelectedTemplateId { get; set; }
        public IContent Content { get; set; }
        public string SessionKey { get; set; }
    }
}