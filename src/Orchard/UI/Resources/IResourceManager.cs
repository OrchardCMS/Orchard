using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterMeta(string name, string content);
        void RegisterStyle(string fileName);
        void RegisterHeadScript(string fileName);
        void RegisterFootScript(string fileName);
        MvcHtmlString GetMetas();
        MvcHtmlString GetStyles(RequestContext requestContext);
        MvcHtmlString GetHeadScripts(RequestContext requestContext);
        MvcHtmlString GetFootScripts(RequestContext requestContext);
    }
}