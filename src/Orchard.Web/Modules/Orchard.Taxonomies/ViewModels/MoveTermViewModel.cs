using System.Collections.Generic;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.ViewModels
{
    public class MoveTermViewModel
    {
        public IEnumerable<TermPart> Terms { get; set; }
        public int SelectedTermId { get; set; }
        public IEnumerable<int> TermIds { get; set; }
    }
}
