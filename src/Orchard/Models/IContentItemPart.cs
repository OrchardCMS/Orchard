namespace Orchard.Models {
    public interface IContentItemPart {
        ContentItem ContentItem { get; set; }
        //int Id { get; }
        //string ModelType { get; }

        //bool Is<T>() where T : class, IContentItemPart;
        //T As<T>() where T : class, IContentItemPart;

        //void Weld(IContentItemPart part);
    }
}
