using System.Web.Mvc;
using System.Web.Routing;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterMeta(string name, string content);
        void RegisterStyle(string fileName, HtmlHelper html);
        void RegisterHeadScript(string fileName, HtmlHelper html);
        void RegisterFootScript(string fileName, HtmlHelper html);
        MvcHtmlString GetMetas();
        MvcHtmlString GetStyles();
        MvcHtmlString GetHeadScripts();
        MvcHtmlString GetFootScripts();
    }
}