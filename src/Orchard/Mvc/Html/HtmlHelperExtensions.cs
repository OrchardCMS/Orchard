using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.Localization;
using Orchard.Utility;
using Orchard.Utility.Extensions;
using System.Web;

namespace Orchard.Mvc.Html {
    public static class HtmlHelperExtensions {

        public static string NameOf<T>(this HtmlHelper<T> html, Expression<Action<T>> expression) {
            return Reflect.NameOf(html.ViewData.Model, expression);
        }

        public static string NameOf<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression) {
            return Reflect.NameOf(html.ViewData.Model, expression);
        }

        public static string FieldNameFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression) {
            return html.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        }

        public static string FieldIdFor<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression) {
            var id = html.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
            // because "[" and "]" aren't replaced with "_" in GetFullHtmlFieldId
            return id.Replace('[', '_').Replace(']', '_');
        }

        public static IHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, LocalizedString labelText) {
            return LabelFor(html, expression, labelText.ToString());
        }

        public static IHtmlString LabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string labelText) {
            if (String.IsNullOrEmpty(labelText)) {
                return MvcHtmlString.Empty;
            }
            var htmlFieldName = ExpressionHelper.GetExpressionText(expression);
            var tag = new TagBuilder("label");
            tag.Attributes.Add("for", html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));
            tag.SetInnerText(labelText);
            return MvcHtmlString.Create(tag.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString SelectOption<T>(this HtmlHelper html, T currentValue, T optionValue, string text) {
            return SelectOption(html, optionValue, object.Equals(optionValue, currentValue), text);
        }

        public static MvcHtmlString SelectOption<T>(this HtmlHelper html, T currentValue, T optionValue, string text, object htmlAttributes) {
            return SelectOption(html, optionValue, object.Equals(optionValue, currentValue), text, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString SelectOption<T>(this HtmlHelper html, T currentValue, T optionValue, string text, RouteValueDictionary htmlAttributes) {
            return SelectOption(html, optionValue, object.Equals(optionValue, currentValue), text, htmlAttributes);
        }

        public static MvcHtmlString SelectOption(this HtmlHelper html, object optionValue, bool selected, string text) {
            return SelectOption(html, optionValue, selected, text, null);
        }

        public static MvcHtmlString SelectOption(this HtmlHelper html, object optionValue, bool selected, string text, object htmlAttributes) {
            return SelectOption(html, optionValue, selected, text, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString SelectOption(this HtmlHelper html, object optionValue, bool selected, string text, RouteValueDictionary htmlAttributes) {
            var builder = new TagBuilder("option");

            if (optionValue != null)
                builder.MergeAttribute("value", optionValue.ToString());

            if (selected)
                builder.MergeAttribute("selected", "selected");

            builder.SetInnerText(text);

            if (htmlAttributes != null) {
                builder.MergeAttributes(htmlAttributes);
            }

            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));
        }

        public static WorkContext GetWorkContext(this HtmlHelper html) {
            var workContext = html.ViewContext.RequestContext.GetWorkContext();

            if (workContext == null)
                throw new ApplicationException("The WorkContext cannot be found for the request");

            return workContext;
        }

        #region UnorderedList

        public static IHtmlString UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, MvcHtmlString> generateContent, string cssClass) {
            return htmlHelper.UnorderedList(items, generateContent, cssClass, null, null);
        }

        public static IHtmlString UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, MvcHtmlString> generateContent, string cssClass, string itemCssClass, string alternatingItemCssClass) {
            return UnorderedList(items, (t, i) => generateContent(t, i) as IHtmlString, cssClass, t => itemCssClass, t => alternatingItemCssClass);
        }

        public static IHtmlString UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, IHtmlString> generateContent, string cssClass) {
            return htmlHelper.UnorderedList(items, generateContent, cssClass, null, null);
        }

        public static IHtmlString UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, IHtmlString> generateContent, string cssClass, string itemCssClass, string alternatingItemCssClass) {
            return UnorderedList(items, generateContent, cssClass, t => itemCssClass, t => alternatingItemCssClass);
        }

        private static IHtmlString UnorderedList<T>(IEnumerable<T> items, Func<T, int, IHtmlString> generateContent, string cssClass, Func<T, string> generateItemCssClass, Func<T, string> generateAlternatingItemCssClass) {
            if(items == null) {
                return new HtmlString(string.Empty);
            }

            // prevent multiple evaluations of the enumeration
            items = items.ToArray();

            if (!items.Any()) {
                return new HtmlString(string.Empty);
            }

            var sb = new StringBuilder(250);
            int counter = 0, count = items.Count() - 1;

            if(string.IsNullOrEmpty(cssClass)) {
                sb.Append("<ul>");
            }
            else {
                sb.Append("<ul class=\"")
                  .Append(cssClass)
                  .Append("\">");
            }

            foreach (var item in items) {
                var sbClass = new StringBuilder(50);

                if (counter == 0) {
                    sbClass.Append("first ");
                }

                if (counter == count) {
                    sbClass.Append("last ");
                }

                if (generateItemCssClass != null) {
                    sbClass.Append(generateItemCssClass(item)).Append(" ");
                }

                if (counter % 2 != 0 && generateAlternatingItemCssClass != null) {
                    sbClass.Append(generateAlternatingItemCssClass(item)).Append(" ");
                }

                var clss = sbClass.ToString().TrimEnd();

                if(String.IsNullOrWhiteSpace(clss)) {
                    sb.Append("<li>")
                      .Append(generateContent(item, counter))
                      .Append("</li>");
                }
                else {
                    sb.Append("<li class=\"")
                      .Append(clss)
                      .Append("\">")
                      .Append(generateContent(item, counter))
                      .Append("</li>");
                }

                counter++;
            }

            sb.Append("</ul>");

            return new HtmlString(sb.ToString());
        }

        #endregion

        #region Ellipsize

        public static IHtmlString Ellipsize(this HtmlHelper htmlHelper, string text, int characterCount) {
            return new HtmlString(htmlHelper.Encode(text.Ellipsize(characterCount)));
        }

        public static IHtmlString Ellipsize(this HtmlHelper htmlHelper, string text, int characterCount, string ellipsis) {
            return new HtmlString(htmlHelper.Encode(text.Ellipsize(characterCount, ellipsis)));
        }

        #endregion

        #region Excerpt

        public static MvcHtmlString Excerpt(this HtmlHelper html, string markup, int length) {
            return MvcHtmlString.Create(html.Encode(HttpUtility.HtmlDecode(markup.RemoveTags()).Ellipsize(length)));
        }

        #endregion

        #region Image

        public static MvcHtmlString Image(this HtmlHelper htmlHelper, string src, string alt, object htmlAttributes) {
            return htmlHelper.Image(src, alt, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString Image(this HtmlHelper htmlHelper, string src, string alt, IDictionary<string, object> htmlAttributes) {
            UrlHelper url = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            string imageUrl = url.Content(src);
            TagBuilder imageTag = new TagBuilder("img");

            if (!string.IsNullOrEmpty(imageUrl))
                imageTag.MergeAttribute("src", imageUrl);

            if (!string.IsNullOrEmpty(alt))
                imageTag.MergeAttribute("alt", alt);

            imageTag.MergeAttributes(htmlAttributes, true);

            if (imageTag.Attributes.ContainsKey("alt") && !imageTag.Attributes.ContainsKey("title"))
                imageTag.MergeAttribute("title", imageTag.Attributes["alt"] ?? "");

            return MvcHtmlString.Create(imageTag.ToString(TagRenderMode.SelfClosing));
        }

        #endregion

        #region Link

        public static IHtmlString Link(this HtmlHelper htmlHelper, string linkContents, string href) {
            return htmlHelper.Link(linkContents, href, null);
        }

        public static IHtmlString Link(this HtmlHelper htmlHelper, IHtmlString linkContents, string href) {
            return htmlHelper.Link(linkContents, href, null);
        }

        public static IHtmlString Link(this HtmlHelper htmlHelper, string linkContents, string href, object htmlAttributes) {
            return htmlHelper.Link(linkContents, href, new RouteValueDictionary(htmlAttributes));
        }

        public static IHtmlString Link(this HtmlHelper htmlHelper, string linkContents, string href, IDictionary<string, object> htmlAttributes) {
            var tagBuilder = new TagBuilder("a") { InnerHtml = htmlHelper.Encode(linkContents) };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", href);
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

        public static IHtmlString Link(this HtmlHelper htmlHelper, IHtmlString linkContents, string href, IDictionary<string, object> htmlAttributes) {
            var tagBuilder = new TagBuilder("a") { InnerHtml = linkContents.ToHtmlString() };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", href);
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

        #endregion

        #region LinkOrDefault

        public static IHtmlString LinkOrDefault(this HtmlHelper htmlHelper, string linkContents, string href) {
            return htmlHelper.LinkOrDefault(linkContents, href, null);
        }

        public static IHtmlString LinkOrDefault(this HtmlHelper htmlHelper, string linkContents, string href, object htmlAttributes) {
            return htmlHelper.LinkOrDefault(linkContents, href, new RouteValueDictionary(htmlAttributes));
        }

        public static IHtmlString LinkOrDefault(this HtmlHelper htmlHelper, string linkContents, string href, IDictionary<string, object> htmlAttributes) {
            string linkText = htmlHelper.Encode(linkContents);

            if (string.IsNullOrEmpty(href)) {
                return new HtmlString(linkText);
            }

            TagBuilder tagBuilder = new TagBuilder("a") {
                InnerHtml = linkText
            };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", href);
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

        #endregion

        #region Hint
        public static IHtmlString Hint(this HtmlHelper htmlHelper, LocalizedString text) {
            return Hint(htmlHelper, text, default(object));
        }

        public static IHtmlString Hint(this HtmlHelper htmlHelper, LocalizedString text, object htmlAttributes) {
            return Hint(htmlHelper, text, htmlAttributes != null ? new RouteValueDictionary(htmlAttributes) : null);
        }

        public static IHtmlString Hint(this HtmlHelper htmlHelper, LocalizedString text, IDictionary<string, object> htmlAttributes) {
            var tagBuilder = new TagBuilder("span") { InnerHtml = text.Text };

            if (htmlAttributes != null) {
                tagBuilder.MergeAttributes(htmlAttributes);
            }

            tagBuilder.AddCssClass("hint");
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }
        #endregion

        
        #region BeginFormAntiForgeryPost

        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper) {
            return htmlHelper.BeginFormAntiForgeryPost(htmlHelper.ViewContext.HttpContext.Request.Url.PathAndQuery, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction) {
            return htmlHelper.BeginFormAntiForgeryPost(formAction, FormMethod.Post, new RouteValueDictionary());
        }

        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod) {
            return htmlHelper.BeginFormAntiForgeryPost(formAction, formMethod, new RouteValueDictionary());
        }

        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod, object htmlAttributes) {
            return htmlHelper.BeginFormAntiForgeryPost(formAction, formMethod, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
        }

        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod, IDictionary<string, object> htmlAttributes) {
            // Force the browser not to cache protected forms, and to reload them if needed.
            var response = htmlHelper.ViewContext.HttpContext.Response;
            response.Cache.SetExpires(System.DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();

            var tagBuilder = new TagBuilder("form");

            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("action", formAction);
            tagBuilder.MergeAttribute("method", HtmlHelper.GetFormMethodString(formMethod), true);

            htmlHelper.ViewContext.Writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));

            return new MvcFormAntiForgeryPost(htmlHelper);
        }
        #endregion

        #region AntiForgeryTokenOrchard

        public static MvcHtmlString AntiForgeryTokenOrchard(this HtmlHelper htmlHelper) {

            try {
                return htmlHelper.AntiForgeryToken();
            }
            catch (HttpAntiForgeryException) {
                // Work-around an issue in MVC 2:  If the browser sends a cookie that is not
                // coming from this server (this can happen if the user didn't close their browser
                // while the application server configuration changed), clear it up
                // so that a new one is generated and sent to the browser. This is harmless
                // from a security point of view, since we are _issuing_ an anti-forgery token,
                // not validating input.

                // Remove the token so that MVC will create a new one.
                var antiForgeryTokenName = htmlHelper.GetAntiForgeryTokenName();
                htmlHelper.ViewContext.HttpContext.Request.Cookies.Remove(antiForgeryTokenName);

                // Try again
                return htmlHelper.AntiForgeryToken();
            }
        }

        private static string GetAntiForgeryTokenName(this HtmlHelper htmlHelper) {
            // Generate the same cookie name as MVC
            var appPath = htmlHelper.ViewContext.HttpContext.Request.ApplicationPath;
            const string antiForgeryTokenName = "__RequestVerificationToken";
            if (string.IsNullOrEmpty(appPath)) {
                return antiForgeryTokenName;
            }
            return antiForgeryTokenName + '_' + Base64EncodeForCookieName(appPath);
        }

        private static string Base64EncodeForCookieName(string s) {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s)).Replace('+', '.').Replace('/', '-').Replace('=', '_');
        }

        #endregion

        #region AntiForgeryTokenValueOrchardLink

        public static IHtmlString AntiForgeryTokenValueOrchardLink(this HtmlHelper htmlHelper, string linkContents, string href) {
            return htmlHelper.AntiForgeryTokenValueOrchardLink(linkContents, href, (object)null);
        }

        public static IHtmlString AntiForgeryTokenValueOrchardLink(this HtmlHelper htmlHelper, string linkContents, string href, object htmlAttributes) {
            return htmlHelper.AntiForgeryTokenValueOrchardLink(linkContents, href, new RouteValueDictionary(htmlAttributes));
        }

        public static IHtmlString AntiForgeryTokenValueOrchardLink(this HtmlHelper htmlHelper, string linkContents, string href, IDictionary<string, object> htmlAttributes) {
            return htmlHelper.Link(linkContents, htmlHelper.AntiForgeryTokenGetUrl(href).ToString(), htmlAttributes);
        }

        #endregion

        #region AntiForgeryTokenGetUrl

        public static IHtmlString AntiForgeryTokenGetUrl(this HtmlHelper htmlHelper, string baseUrl) {
            return new HtmlString(string.Format("{0}{1}__RequestVerificationToken={2}", baseUrl, baseUrl.IndexOf('?') > -1 ? "&" : "?", htmlHelper.ViewContext.HttpContext.Server.UrlEncode(htmlHelper.AntiForgeryTokenValueOrchard().ToString())));
        }

        #endregion

        #region AntiForgeryTokenValueOrchard

        public static IHtmlString AntiForgeryTokenValueOrchard(this HtmlHelper htmlHelper) {
            //HAACK: (erikpo) Since MVC doesn't expose any of its methods for generating the antiforgery token and setting the cookie, we'll just let it do its thing and parse out what we need
            var field = htmlHelper.AntiForgeryTokenOrchard().ToHtmlString();
            var beginIndex = field.IndexOf("value=\"") + 7;
            var endIndex = field.IndexOf("\"", beginIndex);

            return new HtmlString(field.Substring(beginIndex, endIndex - beginIndex));
        }

        #endregion

        #region HtmlHelperFor
        // Credit: Max Toro http://maxtoroq.github.io/2012/07/patterns-for-aspnet-mvc-plugins-viewmodels.html
        public static HtmlHelper<TModel> HtmlHelperFor<TModel>(this HtmlHelper htmlHelper) {
            return HtmlHelperFor(htmlHelper, default(TModel));
        }

        public static HtmlHelper<TModel> HtmlHelperFor<TModel>(this HtmlHelper htmlHelper, TModel model) {
            return HtmlHelperFor(htmlHelper, model, null);
        }

        public static HtmlHelper<TModel> HtmlHelperFor<TModel>(this HtmlHelper htmlHelper, TModel model, string htmlFieldPrefix) {

            var viewDataContainer = CreateViewDataContainer(htmlHelper.ViewData, model);

            var templateInfo = viewDataContainer.ViewData.TemplateInfo;

            if (!String.IsNullOrEmpty(htmlFieldPrefix))
                templateInfo.HtmlFieldPrefix = templateInfo.GetFullHtmlFieldName(htmlFieldPrefix);

            var viewContext = htmlHelper.ViewContext;
            var newViewContext = new ViewContext(
                viewContext.Controller.ControllerContext, 
                viewContext.View, 
                viewDataContainer.ViewData, 
                viewContext.TempData, 
                viewContext.Writer);

            return new HtmlHelper<TModel>(newViewContext, viewDataContainer, htmlHelper.RouteCollection);
        }

        private static IViewDataContainer CreateViewDataContainer(ViewDataDictionary viewData, object model) {

            var newViewData = new ViewDataDictionary(viewData) {
                Model = model
            };

            newViewData.TemplateInfo = new TemplateInfo {
                HtmlFieldPrefix = newViewData.TemplateInfo.HtmlFieldPrefix
            };

            return new ViewDataContainer {
                ViewData = newViewData
            };
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataDictionary ViewData { get; set; }
        }
        #endregion
    }
}
