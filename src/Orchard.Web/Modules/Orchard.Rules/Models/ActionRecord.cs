namespace Orchard.Rules.Models {
    public class ActionRecord {
        public virtual int Id { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual string Parameters { get; set; }
        public virtual int Position { get; set; }

        // Parent property
        public virtual RuleRecord RuleRecord { get; set; }
    }
}