namespace Orchard.AuditTrail.Services.Models {
    public class DiffNode {
        public DiffType Type { get; set; }
        public string Context { get; set; }
        public string Previous { get; set; }
        public string Current { get; set; }
    }
}