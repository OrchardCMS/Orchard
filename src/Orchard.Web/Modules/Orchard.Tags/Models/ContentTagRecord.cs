namespace Orchard.Tags.Models {
    public class ContentTagRecord {
        public virtual int Id { get; set; }
        public virtual TagRecord TagRecord { get; set; }
        public virtual TagsPartRecord TagsPartRecord { get; set; }
    }
}