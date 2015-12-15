using Orchard.Taxonomies.Models;
using System.Collections.Generic;

namespace Orchard.Taxonomies.ViewModels {
    public class SelectTermViewModel {
        public IEnumerable<TermPart> Terms { get; set; }
        public int SelectedTermId { get; set; }
    }
}
