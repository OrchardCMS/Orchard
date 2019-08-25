using System.Collections.Generic;

namespace Orchard.Taxonomies.Models {
    public class MoveTermsContext {
        public IList<TermPart> Terms { get; set; }
        public TermPart ParentTerm { get; set; }
    }
}