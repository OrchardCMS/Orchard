namespace Orchard.Models {
    public class ContentPartForRecord<TRecord> : ContentItemPart {
        public TRecord Record { get; set; }
    }
}