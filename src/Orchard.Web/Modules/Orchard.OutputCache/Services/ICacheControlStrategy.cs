using System.Web;
using System.Web.Mvc;

namespace Orchard.OutputCache.Services {
    /// <summary>
    /// Represents the logic deciding if the result of an action can be cached
    /// </summary>
    public interface ICacheControlStrategy : IDependency {
        bool IsCacheable(ActionResult result, HttpResponseBase response);
    }
}