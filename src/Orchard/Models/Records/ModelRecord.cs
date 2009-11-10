namespace Orchard.Models.Records {
    public class ModelRecord {
        public virtual int Id { get; set; }
        public virtual ModelTypeRecord ModelType { get; set; }
    }
}