namespace Orchard.Models.Records {
    public abstract class ContentPartRecord : ContentPart {
        public virtual int Id { get; set; }
        public virtual ContentItemRecord ContentItemRecord { get; set; }
    }
}
