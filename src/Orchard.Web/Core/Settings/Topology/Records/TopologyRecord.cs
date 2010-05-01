using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Topology.Records {
    public class TopologyRecord {
        public TopologyRecord() {
            EnabledFeatures=new List<TopologyFeatureRecord>();
            Parameters=new List<TopologyParameterRecord>();
        }

        public virtual int Id { get; set; }
        public virtual int SerialNumber { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<TopologyFeatureRecord> EnabledFeatures { get; set; }
        
        [CascadeAllDeleteOrphan]
        public virtual IList<TopologyParameterRecord> Parameters { get; set; }
    }
}
