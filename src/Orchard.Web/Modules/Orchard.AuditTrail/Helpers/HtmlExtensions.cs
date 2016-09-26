using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Orchard.AuditTrail.Helpers {
    public static class HtmlExtensions {
        public static IHtmlString ContentPartEditLink(this HtmlHelper html, string contentPartName) {
            return html.ActionLink(contentPartName, "EditPart", "Admin", new {id = contentPartName, area = "Orchard.ContentTypes"}, null);
        }

        public static IHtmlString ContentTypeEditLink(this HtmlHelper html, string contentTypeName) {
            return html.ActionLink(contentTypeName, "Edit", "Admin", new { id = contentTypeName, area = "Orchard.ContentTypes" }, null);
        }

        public static IHtmlString ItemEditLink(this HtmlHelper html, string linkText, int contentItemId) {
            return html.ActionLink(linkText, "Edit", "Admin", new { id = contentItemId, area = "Contents" }, null);
        }
    }
}