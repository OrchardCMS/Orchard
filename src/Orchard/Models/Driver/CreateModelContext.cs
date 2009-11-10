using Orchard.Models.Records;

namespace Orchard.Models.Driver {
    public class CreateModelContext {
        public int Id { get; set; }
        public string ModelType { get; set; }
        public ModelRecord Record { get; set; }
        public IModel Instance { get; set; }
    }
}