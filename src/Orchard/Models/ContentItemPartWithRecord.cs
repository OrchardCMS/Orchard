namespace Orchard.Models {
    public abstract class ContentItemPartWithRecord<TRecord> : ContentItemPart {
        public TRecord Record { get; set; }
    }
}