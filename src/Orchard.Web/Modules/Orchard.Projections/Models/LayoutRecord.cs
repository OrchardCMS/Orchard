using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.Data.Conventions;

namespace Orchard.Projections.Models {
    public class LayoutRecord {
        public LayoutRecord() {
            Properties = new List<PropertyRecord>();    
        }

        public virtual int Id { get; set; }
        public virtual string Description { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual string State { get; set; }

        public virtual int Display { get; set; }

        [StringLength(64)]
        public virtual string DisplayType { get; set; }

        // Parent property
        public virtual QueryPartRecord QueryPartRecord { get; set; }

        [CascadeAllDeleteOrphan, Aggregate]
        public virtual IList<PropertyRecord> Properties { get; set; }

        [CascadeAllDeleteOrphan, Aggregate]
        public virtual PropertyRecord GroupProperty { get; set; }

        public enum Displays {
            Content,
            Properties
        }

    }
}