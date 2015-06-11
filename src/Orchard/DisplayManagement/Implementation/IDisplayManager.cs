using System.Web;

namespace Orchard.DisplayManagement.Implementation {
    /// <summary>
    /// Coordinates the rendering of shapes.
    /// This interface isn't used directly - instead you would call through a
    /// DisplayHelper created by the IDisplayHelperFactory interface
    /// </summary>
    public interface IDisplayManager : IDependency {
        IHtmlString Execute(DisplayContext context);
    }
}
