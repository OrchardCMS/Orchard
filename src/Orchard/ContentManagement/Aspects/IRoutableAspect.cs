namespace Orchard.ContentManagement.Aspects {
    public interface IRoutableAspect : ITitleAspect, IAliasAspect {
        string Slug { get; set; }
    }
}
