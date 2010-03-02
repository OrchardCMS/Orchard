using System.Web.Mvc;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterMeta(string name, string content);
        void RegisterStyle(string fileName, HtmlHelper html);
        void RegisterLink(LinkEntry entry, HtmlHelper html);
        void RegisterHeadScript(string fileName, HtmlHelper html);
        void RegisterFootScript(string fileName, HtmlHelper html);
        MvcHtmlString GetMetas();
        MvcHtmlString GetStyles();
        MvcHtmlString GetLinks(HtmlHelper html);
        MvcHtmlString GetHeadScripts();
        MvcHtmlString GetFootScripts();
    }
}