namespace Orchard.UI.Models {
    public class ModelTemplate {
        public object Model { get; set; }
        public string Prefix { get; set; }
        public string TemplateName { get; set; }

        public static ModelTemplate For<TModel>(TModel model, string prefix) {
            return new ModelTemplate {
                Model = model,
                Prefix = prefix,
            };
        }

        public static ModelTemplate For<TModel>(TModel model, string prefix, string editorTemplateName) {
            return new ModelTemplate {
                Model = model,
                Prefix = prefix,
                TemplateName = editorTemplateName,
            };
        }
    }
}
