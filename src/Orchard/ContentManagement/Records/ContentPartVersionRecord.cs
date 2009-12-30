namespace Orchard.ContentManagement.Records {
    public abstract class ContentPartVersionRecord {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
        public virtual ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}