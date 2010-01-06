using System.IO;

namespace Orchard.Pages.Services.Templates {
    public class TemplateEntry {
        public string Name { get; set; }
        public TextReader Content { get; set; }
    }
}
