using System.Web.Mvc;
using Orchard.Mvc.Html;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterMeta(string name, string content);
        FileRegistrationContext RegisterStyle(string fileName, HtmlHelper html);
        void RegisterLink(LinkEntry entry, HtmlHelper html);
        FileRegistrationContext RegisterHeadScript(string fileName, HtmlHelper html);
        FileRegistrationContext RegisterFootScript(string fileName, HtmlHelper html);
        MvcHtmlString GetMetas();
        MvcHtmlString GetStyles();
        MvcHtmlString GetLinks(HtmlHelper html);
        MvcHtmlString GetHeadScripts();
        MvcHtmlString GetFootScripts();
    }
}