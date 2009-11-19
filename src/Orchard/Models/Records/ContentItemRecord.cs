namespace Orchard.Models.Records {
    public class ContentItemRecord {
        public virtual int Id { get; set; }
        public virtual ContentTypeRecord ContentType { get; set; }
    }
}