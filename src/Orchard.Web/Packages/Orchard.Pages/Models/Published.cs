namespace Orchard.Pages.Models {
    public class Published {
        public virtual int Id { get; set; }
        public virtual string Slug { get; set; }
        public virtual Page Page { get; set; }
        public virtual PageRevision PageRevision { get; set; }
    }
}
