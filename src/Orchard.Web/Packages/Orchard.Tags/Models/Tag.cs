namespace Orchard.Tags.Models {
    public class Tag {
        public virtual int Id { get; set; }
        public virtual string TagName { get; set; }
    }

    public class TagsContentItems {
        public virtual int Id { get; set; }
        public virtual int TagId { get; set; }
        public virtual int ContentItemId { get; set; }
    }
}
