using System.Collections.Generic;

namespace Orchard.Layouts.Models {
    public class SnippetDescriptor {
        public SnippetDescriptor() {
            Fields = new List<SnippetFieldDescriptor>();
        }

        public IList<SnippetFieldDescriptor> Fields { get; set; }
    }
}