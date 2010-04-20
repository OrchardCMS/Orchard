using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Topology.Records {
    public class TopologyRecord {
        public virtual int Id { get; set; }
        public virtual int SerialNumber { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<TopologyFeatureRecord> EnabledFeatures { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<TopologyParameterRecord> Parameters { get; set; }
    }
}
