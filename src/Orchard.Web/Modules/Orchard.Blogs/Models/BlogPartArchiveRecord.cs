namespace Orchard.Blogs.Models {
    public class BlogPartArchiveRecord {
        public virtual int Id { get; set; }
        public virtual BlogPartRecord BlogPart { get; set; }
        public virtual int Year { get; set; }
        public virtual int Month { get; set; }
        public virtual int PostCount { get; set; }
    }
}