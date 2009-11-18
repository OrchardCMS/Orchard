namespace Orchard.Models.Driver {
    public class ActivatedModelContext {
        public string ModelType { get; set; }
        public IModel Instance { get; set; }
    }
}