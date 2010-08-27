using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ClaySharp;
using Orchard.DisplayManagement;

namespace Orchard.Mvc.Html {
    public class Shapes : IShapeDriver {
        public IHtmlString Link(dynamic Display, object Content, string Url, INamedEnumerable<object> Attributes) {
            var tagBuilder = new TagBuilder("a") { InnerHtml = Display(Content).ToString() };
            tagBuilder.MergeAttributes(Attributes.Named);
            tagBuilder.MergeAttribute("href", Url);
            return Display(new HtmlString(tagBuilder.ToString(TagRenderMode.Normal)));
        }

        public IHtmlString Image(dynamic Display, string Url, INamedEnumerable<object> Attributes) {
            var tagBuilder = new TagBuilder("img");
            tagBuilder.MergeAttributes(Attributes.Named);
            tagBuilder.MergeAttribute("src", Url);
            return Display(new HtmlString(tagBuilder.ToString(TagRenderMode.SelfClosing)));
        }
    }
}
