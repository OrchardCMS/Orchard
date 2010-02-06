namespace Orchard.Blogs.Models {
    public class BlogArchiveRecord {
        public virtual int Id { get; set; }
        public virtual BlogRecord Blog { get; set; }
        public virtual int Year { get; set; }
        public virtual int Month { get; set; }
        public virtual int PostCount { get; set; }
    }
}