using System.Web.Routing;

namespace Orchard.Mvc {
    public interface IHasRequestContext {
        RequestContext RequestContext { get; }
    }
}