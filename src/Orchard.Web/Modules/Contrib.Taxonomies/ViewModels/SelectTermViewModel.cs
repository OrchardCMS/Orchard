using Contrib.Taxonomies.Models;
using System.Collections.Generic;

namespace Contrib.Taxonomies.ViewModels {
    public class SelectTermViewModel {
        public IEnumerable<TermPart> Terms { get; set; }
        public int SelectedTermId { get; set; }
    }
}
