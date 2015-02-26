namespace Orchard.Layouts.Framework.Elements {
    public class ElementSavingContext : LayoutSavingContext {
        public ElementSavingContext(LayoutSavingContext stub) {
            Content = stub.Content;
            Updater = stub.Updater;
            Elements = stub.Elements;
        }
        public Element Element { get; set; }
    }
}