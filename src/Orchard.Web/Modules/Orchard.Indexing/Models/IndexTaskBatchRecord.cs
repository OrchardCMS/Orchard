namespace Orchard.Indexing.Models {
    public class IndexTaskBatchRecord {
        public virtual int Id { get; set; }
        public virtual int BatchStartIndex { get; set; }
        public virtual string ContentType { get; set; }
    }
}