using System.Collections.Generic;
using Orchard.Localization;

namespace Orchard.Layouts.Models {
    public class SnippetDescriptor {
        public string Category { get; set; }
        public LocalizedString Description { get; set; }
        public LocalizedString DisplayName { get; set; }
        public string ToolboxIcon { get; set; }


        public SnippetDescriptor() {
            Fields = new List<SnippetFieldDescriptor>();
        }


        public IList<SnippetFieldDescriptor> Fields { get; set; }
    }
}