namespace Orchard.Models.Driver {
    public class NewModelContext {
        public string ModelType { get; set; }
        public ModelBuilder Builder { get; set; }
    }
    public class NewedModelContext {
        public string ModelType { get; set; }
        public IModel Instance { get; set; }
    }
}
