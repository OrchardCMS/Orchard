namespace Orchard.Models.Driver {
    public class ActivatingModelContext {
        public string ModelType { get; set; }
        public ModelBuilder Builder { get; set; }
    }
    public class ActivatedModelContext {
        public string ModelType { get; set; }
        public IModel Instance { get; set; }
    }
}
