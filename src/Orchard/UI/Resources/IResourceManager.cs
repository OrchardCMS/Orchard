using System.Web.Mvc;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterMeta(string name, string content);
        void RegisterStyle(string fileName);
        void RegisterHeadScript(string fileName);
        void RegisterFootScript(string fileName);
        MvcHtmlString GetMetas();
        MvcHtmlString GetStyles();
        MvcHtmlString GetHeadScripts();
        MvcHtmlString GetFootScripts();
    }
}