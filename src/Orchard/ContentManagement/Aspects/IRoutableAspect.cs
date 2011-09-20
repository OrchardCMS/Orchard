namespace Orchard.ContentManagement.Aspects {
    public interface IRoutableAspect : ITitleAspect {
        string Slug { get; set; }
        string Path { get; set; }
    }
}
