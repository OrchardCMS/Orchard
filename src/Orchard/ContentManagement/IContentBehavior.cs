using ClaySharp;

namespace Orchard.ContentManagement {
    public interface IContentBehavior {
        IClayBehavior Behavior { get; }
    }
}