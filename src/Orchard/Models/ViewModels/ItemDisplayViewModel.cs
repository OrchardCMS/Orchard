using System.Collections.Generic;

namespace Orchard.Models.ViewModels {
    public class ItemDisplayViewModel  {
        public ContentItem ContentItem { get; set; }
        public string TemplateName { get; set; }
        public IEnumerable<TemplateViewModel> Displays { get; set; }
    }
}