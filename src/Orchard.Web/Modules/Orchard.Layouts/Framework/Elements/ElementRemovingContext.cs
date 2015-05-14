namespace Orchard.Layouts.Framework.Elements {
    public class ElementRemovingContext : LayoutSavingContext {
        public ElementRemovingContext(LayoutSavingContext stub) {
            Content = stub.Content;
            RemovedElements = stub.RemovedElements;
            Updater = stub.Updater;
            Elements = stub.Elements;
        }
        public Element Element { get; set; }
    }
}