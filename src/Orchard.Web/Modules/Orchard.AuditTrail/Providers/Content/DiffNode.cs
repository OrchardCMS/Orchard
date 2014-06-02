namespace Orchard.AuditTrail.Providers.Content {
    public class DiffNode {
        public DiffType Type { get; set; }
        public string ElementName { get; set; }
        public string Previous { get; set; }
        public string Current { get; set; }
    }
}