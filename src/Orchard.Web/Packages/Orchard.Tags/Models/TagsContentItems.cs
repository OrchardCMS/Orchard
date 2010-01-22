namespace Orchard.Tags.Models {
    public class TagsContentItems {
        public virtual int Id { get; set; }
        public virtual int TagId { get; set; }
        public virtual int ContentItemId { get; set; }
    }
}