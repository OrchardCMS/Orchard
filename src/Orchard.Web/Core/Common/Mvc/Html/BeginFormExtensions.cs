using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Orchard.Core.Common.Mvc.Html {
    public static class BeginFormExtensions {
        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper)
        {
            return htmlHelper.BeginFormAntiForgeryPost(htmlHelper.ViewContext.HttpContext.Request.RawUrl, FormMethod.Post, new RouteValueDictionary());
        }
        //TODO: (erikpo) Uncomment when needed (not currently needed)
        //public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction) {
        //    return htmlHelper.BeginFormAntiForgeryPost(formAction, FormMethod.Post, new RouteValueDictionary());
        //}
        //public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod) {
        //    return htmlHelper.BeginFormAntiForgeryPost(formAction, formMethod, new RouteValueDictionary());
        //}
        //public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod, object htmlAttributes) {
        //    return htmlHelper.BeginFormAntiForgeryPost(formAction, formMethod, new RouteValueDictionary(htmlAttributes));
        //}
        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("form");

            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("action", formAction);
            tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(formMethod), true);

            htmlHelper.ViewContext.HttpContext.Response.Output.Write(tagBuilder.ToString(TagRenderMode.StartTag));

            return new MvcFormAntiForgeryPost(htmlHelper);
        }
    }
}