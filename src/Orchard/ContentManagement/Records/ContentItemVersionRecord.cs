namespace Orchard.ContentManagement.Records {
    public class ContentItemVersionRecord {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
        public virtual int Number { get; set; }

        public virtual bool Published { get; set; }
        public virtual bool Latest { get; set; }
    }
}