using System.Collections.Generic;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace Orchard.Taxonomies.Models {
    public class TermsPartRecord : ContentPartRecord {
        public TermsPartRecord() {
            Terms = new List<TermContentItem>();
        }

        [CascadeAllDeleteOrphan]
        public virtual IList<TermContentItem> Terms { get; set; }
    }
}