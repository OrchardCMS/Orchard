using System.Collections.Generic;

namespace Orchard.Models.ViewModels {
    public class ItemEditorViewModel  {
        public ContentItem ContentItem { get; set; }
        public string TemplateName { get; set; }
        public IEnumerable<TemplateViewModel> Editors { get; set; }
    }
}