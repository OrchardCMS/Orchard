using System.Web.Mvc;
using Orchard.Mvc.Html;

namespace Orchard.UI.Resources {
    public interface IResourceManager : IDependency {
        void RegisterMeta(string name, string content);
        FileRegistrationContext RegisterStyle(string fileName, HtmlHelper html);
        FileRegistrationContext RegisterStyle(string fileName, HtmlHelper html, string position);
        void RegisterLink(LinkEntry entry, HtmlHelper html);
        FileRegistrationContext RegisterHeadScript(string fileName, HtmlHelper html);
        FileRegistrationContext RegisterHeadScript(string fileName, HtmlHelper html, string position);
        FileRegistrationContext RegisterFootScript(string fileName, HtmlHelper html);
        FileRegistrationContext RegisterFootScript(string fileName, HtmlHelper html, string position);
        MvcHtmlString GetMetas();
        MvcHtmlString GetStyles();
        MvcHtmlString GetLinks(HtmlHelper html);
        MvcHtmlString GetHeadScripts();
        MvcHtmlString GetFootScripts();
    }
}