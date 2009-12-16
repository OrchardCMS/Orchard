using System.Web.Mvc;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterStyle(string fileName);
        void RegisterHeadScript(string fileName);
        void RegisterFootScript(string fileName);
        MvcHtmlString GetStyles();
        MvcHtmlString GetHeadScripts();
        MvcHtmlString GetFootScripts();
    }
}