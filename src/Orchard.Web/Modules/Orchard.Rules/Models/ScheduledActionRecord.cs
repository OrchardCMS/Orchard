using Orchard.Data.Conventions;

namespace Orchard.Rules.Models {
    public class ScheduledActionRecord {
        public virtual int Id { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual ActionRecord ActionRecord { get; set; }
    }
}