namespace Orchard.ContentManagement.Parts {
    public interface IRoutePart : IContent {
        string Title { get; set; }
        string Slug { get; set; }
        string Path { get; set; }
    }
}
