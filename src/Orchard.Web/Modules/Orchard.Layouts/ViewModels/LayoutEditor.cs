using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.ViewModels {
    public class LayoutEditor {
        public IContent Content { get; set; }
        public string Data { get; set; }
        public string ConfigurationData { get; set; }
        public string Trash { get; set; }
        public int? TemplateId { get; set; }
        public string SessionKey { get; set; }
        public IList<LayoutPart> Templates { get; set; }
    }
}