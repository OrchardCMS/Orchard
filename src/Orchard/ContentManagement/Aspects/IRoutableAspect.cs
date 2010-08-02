namespace Orchard.ContentManagement.Aspects {
    public interface IRoutableAspect : IContent {
        string Title { get; set; }
        string Slug { get; set; }
        string Path { get; set; }
    }
}
