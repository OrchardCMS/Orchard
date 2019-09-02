using System.Collections.Generic;
using Orchard.Layouts.Models;

namespace Orchard.Layouts.ViewModels {
    public class SnippetViewModel {
        public SnippetViewModel() {
            FieldEditors = new List<dynamic>();
        }

        public SnippetDescriptor Descriptor { get; set; }
        public IList<dynamic> FieldEditors { get; set; }
    }
}