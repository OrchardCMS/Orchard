namespace Orchard.Models {
    public abstract class ModelPartWithRecord<TRecord> : ModelPart {
        public TRecord Record { get; set; }
    }
}