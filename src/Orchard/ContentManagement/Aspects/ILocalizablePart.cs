namespace Orchard.ContentManagement.Aspects {
    public interface ILocalizablePart : IContent {
        string Culture { get ; }
    }
}
