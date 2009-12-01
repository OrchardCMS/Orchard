namespace Orchard.UI.Models {
    public class ModelTemplate {
        public ModelTemplate(object model)
            : this(model, string.Empty) {
        }
        public ModelTemplate(object model, string prefix) {
            Model = model;
            Prefix = prefix;
        }


        public object Model { get; set; }
        public string Prefix { get; set; }
        public string TemplateName { get; set; }

    }
}
