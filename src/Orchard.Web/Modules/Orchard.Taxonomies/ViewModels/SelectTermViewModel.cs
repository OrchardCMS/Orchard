using System.Collections.Generic;
using Orchard.Taxonomies.Models;

namespace Orchard.Taxonomies.ViewModels
{
    public class SelectTermViewModel
    {
        public IEnumerable<TermPart> Terms { get; set; }
        public int SelectedTermId { get; set; }
    }
}
