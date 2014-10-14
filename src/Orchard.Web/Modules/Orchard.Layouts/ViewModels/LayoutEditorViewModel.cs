using System.Collections.Generic;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.ViewModels {
    public class LayoutEditorViewModel {
        public LayoutPart Part { get; set; }
        public string State { get; set; }
        public dynamic LayoutRoot { get; set; }
        public IList<LayoutPart> Templates { get; set; }
        public int? SelectedTemplateId { get; set; }
    }
}