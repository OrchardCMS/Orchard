namespace Orchard.Rules.Models {
    public class EventRecord {
        public virtual int Id { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual string Parameters { get; set; }

        // Parent property
        public virtual RuleRecord RuleRecord { get; set; }
    }
}