using System.Web;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Implementation {
    public interface IShapeDisplayEvents : IDependency {
        void Displaying(ShapeDisplayingContext context);
        void Displayed(ShapeDisplayedContext context);
    }

    public class ShapeDisplayingContext {
        public dynamic Shape { get; set; }
        public ShapeMetadata ShapeMetadata { get; set; }
        public IHtmlString ChildContent { get; set; }
    }

    public class ShapeDisplayedContext {
        public dynamic Shape { get; set; }
        public ShapeMetadata ShapeMetadata { get; set; }
        public IHtmlString ChildContent { get; set; }
    }

    public abstract class ShapeDisplayEvents : IShapeDisplayEvents {
        public virtual void Displaying(ShapeDisplayingContext context) { }
        public virtual void Displayed(ShapeDisplayedContext context) { }
    }
}
