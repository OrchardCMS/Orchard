using System.Collections.Generic;

namespace Orchard.Core.Settings.Topology.Records {
    public class TopologyRecord {
        public virtual int Id { get; set; }
        public virtual int SerialNumber { get; set; }
        public virtual IList<TopologyFeatureRecord> EnabledFeatures { get; set; }
        public virtual IList<TopologyParameterRecord> Parameters { get; set; }
    }
}
