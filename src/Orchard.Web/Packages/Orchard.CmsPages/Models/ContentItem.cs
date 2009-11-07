namespace Orchard.CmsPages.Models {
    public partial class ContentItem {
        public virtual int Id { get; set; }
        public virtual PageRevision PageRevision { get; set; }
        public virtual string Content { get; set; }
        public virtual string ZoneName { get; set; }
    }
}
