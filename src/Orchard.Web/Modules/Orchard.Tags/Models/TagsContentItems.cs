using Orchard.ContentManagement.Records;

namespace Orchard.Tags.Models {
    public class TagsContentItems {
        public virtual int Id { get; set; }
        public virtual TagRecord Tag { get; set; }
        public virtual ContentItemRecord ContentItem { get; set; }
    }
}