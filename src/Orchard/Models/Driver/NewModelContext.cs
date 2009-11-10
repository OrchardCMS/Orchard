namespace Orchard.Models.Driver {
    public class NewModelContext {
        public string ModelType { get; set; }
        public IModel Instance { get; set; }
    }
    public class LoadModelContext {
        public int Id { get; set; }
        public string ModelType { get; set; }
        public IModel Instance { get; set; }
    }
    public class CreateModelContext {
        public int Id { get; set; }
        public string ModelType { get; set; }
        public IModel Instance { get; set; }
    }
}
