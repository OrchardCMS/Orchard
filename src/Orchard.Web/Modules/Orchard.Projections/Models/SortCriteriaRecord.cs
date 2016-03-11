namespace Orchard.Projections.Models {
    public class SortCriterionRecord {
        public virtual int Id { get; set; }
        public virtual string Description { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual string State { get; set; }
        public virtual int Position { get; set; }

        // Parent property
        public virtual QueryPartRecord QueryPartRecord { get; set; }
    }
}