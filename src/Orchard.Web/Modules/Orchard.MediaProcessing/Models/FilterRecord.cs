namespace Orchard.MediaProcessing.Models {
    public class FilterRecord {
        public virtual int Id { get; set; }

        public virtual string Description { get; set; }
        public virtual string Category { get; set; }
        public virtual string Type { get; set; }
        public virtual int Position { get; set; }
        public virtual string State { get; set; }

        // Parent property
        public virtual ImageProfilePartRecord ImageProfilePartRecord { get; set; }
    }
}