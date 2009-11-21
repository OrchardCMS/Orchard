using System.Web.Routing;

namespace Orchard.Models {
    public interface IContentItemDisplay : IContentItemPart {
        string DisplayText { get; }
        RouteValueDictionary DisplayRouteValues();
        RouteValueDictionary EditRouteValues();
    }
}