namespace Orchard.Core.Settings.Topology.Records {
    public class TopologyParameterRecord {
        public virtual int Id { get; set; }
        public virtual TopologyRecord TopologyRecord { get; set; }
        public virtual string Component { get; set; }
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
    }
}