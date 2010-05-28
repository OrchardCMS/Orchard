using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.State.Records {
    public class ShellStateRecord {
        public ShellStateRecord() {
            Features = new List<ShellFeatureStateRecord>(); }

        public virtual int Id { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<ShellFeatureStateRecord> Features { get; set; }
    }
}