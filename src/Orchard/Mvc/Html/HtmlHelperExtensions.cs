using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Orchard.Mvc.ViewModels;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Utility;
using Orchard.Utility.Extensions;

namespace Orchard.Mvc.Html {
    public static class HtmlHelperExtensions {
        public static string NameOf<T>(this HtmlHelper<T> html, Expression<Action<T>> expression) {
            return Reflect.NameOf(html.ViewData.Model, expression);
        }

        public static string NameOf<T, TResult>(this HtmlHelper<T> html, Expression<Func<T, TResult>> expression) {
            return Reflect.NameOf(html.ViewData.Model, expression);
        }

        public static MvcHtmlString SelectOption<T>(this HtmlHelper html, T currentValue, T optionValue, string text) {
            return SelectOption(html, optionValue, object.Equals(optionValue, currentValue), text);
        }

        public static MvcHtmlString SelectOption(this HtmlHelper html, object optionValue, bool selected, string text) {
            var builder = new TagBuilder("option");

            if (optionValue != null)
                builder.MergeAttribute("value", optionValue.ToString());

            if (selected)
                builder.MergeAttribute("selected", "selected");

            builder.SetInnerText(text);

            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));
        }

        #region UnorderedList

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, string> generateContent, string cssClass) {
            return htmlHelper.UnorderedList(items, generateContent, cssClass, null, null);
        }

        public static string UnorderedList<T>(this HtmlHelper htmlHelper, IEnumerable<T> items, Func<T, int, string> generateContent, string cssClass, string itemCssClass, string alternatingItemCssClass) {
            return UnorderedList(items, generateContent, cssClass, t => itemCssClass, t => alternatingItemCssClass);
        }

        private static string UnorderedList<T>(IEnumerable<T> items, Func<T, int, string> generateContent, string cssClass, Func<T, string> generateItemCssClass, Func<T, string> generateAlternatingItemCssClass) {
            if (items == null || items.Count() == 0) return "";

            var sb = new StringBuilder(250);
            int counter = 0, count = items.Count() - 1;

            sb.AppendFormat(
                !string.IsNullOrEmpty(cssClass) ? "<ul class=\"{0}\">" : "<ul>",
                cssClass
                );

            foreach (var item in items) {
                var sbClass = new StringBuilder(50);

                if (counter == 0)
                    sbClass.Append("first ");
                if (counter == count)
                    sbClass.Append("last ");
                if (generateItemCssClass != null)
                    sbClass.AppendFormat("{0} ", generateItemCssClass(item));
                if (counter % 2 != 0 && generateAlternatingItemCssClass != null)
                    sbClass.AppendFormat("{0} ", generateAlternatingItemCssClass(item));

                sb.AppendFormat(
                    sbClass.Length > 0
                        ? string.Format("<li class=\"{0}\">{{0}}</li>", sbClass.ToString().TrimEnd())
                        : "<li>{0}</li>",
                    generateContent(item, counter)
                    );

                counter++;
            }

            sb.Append("</ul>");

            return sb.ToString();
        }

        #endregion

        #region Excerpt

        public static MvcHtmlString Excerpt(this HtmlHelper html, string markup, int length) {

            return MvcHtmlString.Create(markup.RemoveTags().Ellipsize(length));
        }

        #endregion

        #region Format Date/Time

        public static string DateTimeRelative(this HtmlHelper htmlHelper, DateTime? value, string defaultIfNull) {
            return value.HasValue ? htmlHelper.DateTimeRelative(value.Value) : defaultIfNull;
        }

        //TODO: (erikpo) This method needs localized
        public static string DateTimeRelative(this HtmlHelper htmlHelper, DateTime value) {
            TimeSpan time = htmlHelper.Resolve<IClock>().UtcNow - value;

            if (time.TotalDays > 7)
                return "at " + htmlHelper.DateTime(value);
            if (time.TotalHours > 24)
                return string.Format("{0} day{1} ago", time.Days, time.Days == 1 ? "" : "s");
            if (time.TotalMinutes > 60)
                return string.Format("{0} hour{1} ago", time.Hours, time.Hours == 1 ? "" : "s");
            if (time.TotalSeconds > 60)
                return string.Format("{0} minute{1} ago", time.Minutes, time.Minutes == 1 ? "" : "s");
            if (time.TotalSeconds > 10)
                return string.Format("{0} second{1} ago", time.Seconds, time.Seconds == 1 ? "" : "s");

            return "a moment ago";
        }

        public static string DateTime(this HtmlHelper htmlHelper, DateTime? value, string defaultIfNull) {
            return value.HasValue ? htmlHelper.DateTime(value.Value) : defaultIfNull;
        }

        public static string DateTime(this HtmlHelper htmlHelper, DateTime? value, string defaultIfNull, string customFormat) {
            return value.HasValue ? htmlHelper.DateTime(value.Value, customFormat) : defaultIfNull;
        }

        public static string DateTime(this HtmlHelper htmlHelper, DateTime value) {
            //TODO: (erikpo) This default format should come from a site setting
            return htmlHelper.DateTime(value, "MMM d yyyy h:mm tt");
        }

        public static string DateTime(this HtmlHelper htmlHelper, DateTime value, string customFormat) {
            //TODO: (erikpo) In the future, convert this to "local" time before calling ToString
            return value.ToString(customFormat);
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

        public static string Link(this HtmlHelper htmlHelper, string linkContents, string href)
        {
            return htmlHelper.Link(linkContents, href, null);
        }

        public static string Link(this HtmlHelper htmlHelper, string linkContents, string href, object htmlAttributes)
        {
            return htmlHelper.Link(linkContents, href, new RouteValueDictionary(htmlAttributes));
        }

        public static string Link(this HtmlHelper htmlHelper, string linkContents, string href, IDictionary<string, object> htmlAttributes)
        {
            TagBuilder tagBuilder = new TagBuilder("a") 
                { InnerHtml = htmlHelper.Encode(linkContents) };
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("href", href);
            return tagBuilder.ToString(TagRenderMode.Normal);
        }

        #endregion

        #region LinkOrDefault

        public static string LinkOrDefault(this HtmlHelper htmlHelper, string linkContents, string href)
        {
            return htmlHelper.LinkOrDefault(linkContents, href, null);
        }

        public static string LinkOrDefault(this HtmlHelper htmlHelper, string linkContents, string href, object htmlAttributes)
        {
            return htmlHelper.LinkOrDefault(linkContents, href, new RouteValueDictionary(htmlAttributes));
        }

        public static string LinkOrDefault(this HtmlHelper htmlHelper, string linkContents, string href, IDictionary<string, object> htmlAttributes)
        {
            if (!string.IsNullOrEmpty(href))
            {
                TagBuilder tagBuilder = new TagBuilder("a")
                {
                    InnerHtml = linkContents
                };
                tagBuilder.MergeAttributes(htmlAttributes);
                tagBuilder.MergeAttribute("href", href);
                linkContents = tagBuilder.ToString(TagRenderMode.Normal);
            }

            return linkContents;
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
            return htmlHelper.BeginFormAntiForgeryPost(formAction, formMethod, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcForm BeginFormAntiForgeryPost(this HtmlHelper htmlHelper, string formAction, FormMethod formMethod, IDictionary<string, object> htmlAttributes) {
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
            var siteSalt = htmlHelper.Resolve<ISiteService>().GetSiteSettings().SiteSalt;

            return htmlHelper.AntiForgeryToken(siteSalt);
        }

        #endregion

        #region AntiForgeryTokenValueOrchardLink

        public static string AntiForgeryTokenValueOrchardLink(this HtmlHelper htmlHelper, string linkContents, string href)  {
            return htmlHelper.AntiForgeryTokenValueOrchardLink(linkContents, href, (object)null);
        }

        public static string AntiForgeryTokenValueOrchardLink(this HtmlHelper htmlHelper, string linkContents, string href, object htmlAttributes)  {
            return htmlHelper.AntiForgeryTokenValueOrchardLink(linkContents, href, new RouteValueDictionary(htmlAttributes));
        }

        public static string AntiForgeryTokenValueOrchardLink(this HtmlHelper htmlHelper, string linkContents, string href, IDictionary<string, object> htmlAttributes)  {
            return htmlHelper.Link(linkContents, htmlHelper.AntiForgeryTokenGetUrl(href), htmlAttributes);
        }

        #endregion

        #region AntiForgeryTokenGetUrl

        public static string AntiForgeryTokenGetUrl(this HtmlHelper htmlHelper, string baseUrl)  {
            return string.Format("{0}{1}__RequestVerificationToken={2}", baseUrl, baseUrl.IndexOf('?') > -1 ? "&" : "?", htmlHelper.ViewContext.HttpContext.Server.UrlEncode(htmlHelper.AntiForgeryTokenValueOrchard()));
        }

        #endregion

        #region AntiForgeryTokenValueOrchard

        public static string AntiForgeryTokenValueOrchard(this HtmlHelper htmlHelper) {
            //HAACK: (erikpo) Since MVC doesn't expose any of its methods for generating the antiforgery token and setting the cookie, we'll just let it do its thing and parse out what we need
            var field = htmlHelper.AntiForgeryTokenOrchard().ToHtmlString();
            var beginIndex = field.IndexOf("value=\"") + 7;
            var endIndex = field.IndexOf("\"", beginIndex);

            return field.Substring(beginIndex, endIndex - beginIndex);
        }

        #endregion

        #region AddRenderAction

        public static void AddRenderAction(this HtmlHelper html, string location, string actionName) {
            AddRenderActionHelper(html, location, actionName, null/*controllerName*/, null);
        }
        public static void AddRenderAction(this HtmlHelper html, string location, string actionName, object routeValues) {
            AddRenderActionHelper(html, location, actionName, null/*controllerName*/, new RouteValueDictionary(routeValues));
        }
        public static void AddRenderAction(this HtmlHelper html, string location, string actionName, RouteValueDictionary routeValues) {
            AddRenderActionHelper(html, location, actionName, null/*controllerName*/, routeValues);
        }
        public static void AddRenderAction(this HtmlHelper html, string location, string actionName, string controllerName) {
            AddRenderActionHelper(html, location, actionName, controllerName, null/*RouteValueDictionary*/);
        }
        public static void AddRenderAction(this HtmlHelper html, string location, string actionName, string controllerName, object routeValues) {
            AddRenderActionHelper(html, location, actionName, controllerName, new RouteValueDictionary(routeValues));
        }
        public static void AddRenderAction(this HtmlHelper html, string location, string actionName, string controllerName, RouteValueDictionary routeValues) {
            AddRenderActionHelper(html, location, actionName, controllerName, routeValues);
        }

        private static void AddRenderActionHelper(this HtmlHelper html, string location, string actionName, string controllerName, RouteValueDictionary routeValues) {
            // Retrieve the "BaseViewModel" for zones if we have one
            var baseViewModel = BaseViewModel.From(html.ViewData);
            if (baseViewModel == null)
                return;

            baseViewModel.Zones.AddRenderAction(location, actionName, controllerName, routeValues);
        }

        #endregion

    }
}
