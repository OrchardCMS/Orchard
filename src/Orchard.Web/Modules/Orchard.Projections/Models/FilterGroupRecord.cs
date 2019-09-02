using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Projections.Models {
    public class FilterGroupRecord{
        public FilterGroupRecord() {
            Filters = new List<FilterRecord>();
        }

        public virtual int Id { get; set; }
        
        [CascadeAllDeleteOrphan, Aggregate]
        public virtual IList<FilterRecord> Filters { get; set; }
        
        // Parent property
        public virtual QueryPartRecord QueryPartRecord { get; set; }
    }
}