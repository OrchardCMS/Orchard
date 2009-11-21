namespace Orchard.Models {
    public class ContentPartForRecord<TRecord> : ContentPart {
        public TRecord Record { get; set; }
    }
}