namespace Orchard.Layouts.Framework.Elements {
    public class ElementIndexingContext : LayoutIndexingContext {
        public ElementIndexingContext(LayoutIndexingContext stub) {
            DocumentIndex = stub.DocumentIndex;
            Layout = stub.Layout;
            Elements = stub.Elements;
        }
        public IElement Element { get; set; }
    }
}