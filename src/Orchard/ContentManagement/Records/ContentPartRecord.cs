using System.ComponentModel.DataAnnotations;
using Orchard.Data.Conventions;

namespace Orchard.ContentManagement.Records {
    public abstract class ContentPartRecord {
        [ScaffoldColumn(false)]
        public virtual int Id { get; set; }
        [CascadeAllDeleteOrphan]
        [ScaffoldColumn(false)]
        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }
}
