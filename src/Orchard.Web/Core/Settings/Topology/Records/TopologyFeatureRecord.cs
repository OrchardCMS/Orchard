namespace Orchard.Core.Settings.Topology.Records {
    public class TopologyFeatureRecord {
        public virtual int Id { get; set; }
        public virtual TopologyRecord TopologyRecord { get; set; }
        public virtual string Name { get; set; }
    }
}