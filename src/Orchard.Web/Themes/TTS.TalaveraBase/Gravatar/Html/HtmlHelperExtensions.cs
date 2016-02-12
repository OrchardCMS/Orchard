using System;
using System.Web.Mvc;
using System.Collections.Generic;

namespace TTS.TalaveraBase.Gravatar.Html {
    /// <summary>
    /// MVC HtmlHelper extension methods for rendering Gravatar avatar images.
    /// </summary>
    public static class HtmlHelperExtenions {

        private static Dictionary<string, string> DictionaryFromObject(object htmlAttributes) {
            var routes = new System.Web.Routing.RouteValueDictionary(htmlAttributes);
            var dict = new Dictionary<string, string>();
            foreach (var kvp in routes)
                dict.Add(kvp.Key, kvp.Value.ToString());
            return dict;
        }

        /// <summary>
        /// Gets an img tag of the Gravatar for the supplied specifications.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <param name="size">The size of the rendered Gravatar.</param>
        /// <param name="defaultImage">The default image to display if no Gravatar exists for the specified <paramref name="email"/>.</param>
        /// <param name="maxRating">The maximum Gravatar rating to allow for rendered Gravatars.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the rendered tag.</param>
        /// <returns>An HTML string of the rendered img tag.</returns>
        public static MvcHtmlString Gravatar(
            this HtmlHelper htmlHelper,
            string email,
            int? size,
            string defaultImage,
            Rating? maxRating,
            IDictionary<string, string> htmlAttributes
        ) {            
            var gravatar = new Gravatar();
            gravatar.DefaultImage = defaultImage;
            if (size.HasValue) gravatar.Size = size.Value;
            if (maxRating.HasValue) gravatar.MaxRating = maxRating.Value;

            return MvcHtmlString.Create(gravatar.Render(email, htmlAttributes));
        }

        /// <summary>
        /// Gets an img tag of the Gravatar for the supplied specifications.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <param name="size">The size of the rendered Gravatar.</param>
        /// <param name="defaultImage">The default image to display if no Gravatar exists for the specified <paramref name="email"/>.</param>
        /// <param name="maxRating">The maximum Gravatar rating to allow for rendered Gravatars.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the rendered tag.</param>
        /// <returns>An HTML string of the rendered img tag.</returns>
        public static MvcHtmlString Gravatar(
            this HtmlHelper htmlHelper,
            string email,
            int? size,
            string defaultImage,
            Rating? maxRating,
            object htmlAttributes
        ) {
            return Gravatar(htmlHelper, email, size, defaultImage, maxRating, DictionaryFromObject(htmlAttributes));
        }

        /// <summary>
        /// Gets an img tag of the Gravatar for the supplied specifications.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <param name="size">The size of the rendered Gravatar.</param>
        /// <returns>An HTML string of the rendered img tag.</returns>
        public static MvcHtmlString Gravatar(
            this HtmlHelper htmlHelper,
            string email,
            int? size
        ) {
            return Gravatar(htmlHelper, email, size, null, null, null);
        }

        /// <summary>
        /// Gets an img tag of the Gravatar for the supplied specifications.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <param name="size">The size of the rendered Gravatar.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the rendered tag.</param>
        /// <returns>An HTML string of the rendered img tag.</returns>
        public static MvcHtmlString Gravatar(
            this HtmlHelper htmlHelper,
            string email,
            int? size,
            IDictionary<string, string> htmlAttributes
        ) {
            return Gravatar(htmlHelper, email, size, null, null, htmlAttributes);
        }

        /// <summary>
        /// Gets an img tag of the Gravatar for the supplied specifications.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="email">The email address whose Gravatar should be rendered.</param>
        /// <param name="size">The size of the rendered Gravatar.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the rendered tag.</param>
        /// <returns>An HTML string of the rendered img tag.</returns>
        public static MvcHtmlString Gravatar(
            this HtmlHelper htmlHelper,
            string email,
            int? size,
            object htmlAttributes
        ) {
            return Gravatar(htmlHelper, email, size, DictionaryFromObject(htmlAttributes));
        }

        /// <summary>
        /// Gets a link tag of the Gravatar profile for the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="linkText">The link text to display.</param>
        /// <param name="email">The email address whose Gravatar profile link should be rendered.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the rendered tag.</param>
        /// <returns>An HTML string of the rendered link tag.</returns>
        public static MvcHtmlString GrofileLink(this HtmlHelper htmlHelper, string linkText, string email, IDictionary<string, string> htmlAttributes) {
            var href = new Grofile().GetLink(email);
            var link = "<a href=\"" + href + "\"";
            if (htmlAttributes != null)
                foreach (var kvp in htmlAttributes)
                    link += " " + kvp.Key + "=\"" + kvp.Value + "\"";
            link += ">" + linkText + "</a>";
            return MvcHtmlString.Create(link);
        }

        /// <summary>
        /// Gets a link tag of the Gravatar profile for the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="linkText">The link text to display.</param>
        /// <param name="email">The email address whose Gravatar profile link should be rendered.</param>
        /// <param name="htmlAttributes">Additional attributes to include in the rendered tag.</param>
        /// <returns>An HTML string of the rendered link tag.</returns>
        public static MvcHtmlString GrofileLink(this HtmlHelper htmlHelper, string linkText, string email, object htmlAttributes) {
            return GrofileLink(htmlHelper, linkText, email, DictionaryFromObject(htmlAttributes));
        }

        /// <summary>
        /// Gets a link tag of the Gravatar profile for the specified <paramref name="email"/>.
        /// </summary>
        /// <param name="linkText">The link text to display.</param>
        /// <param name="htmlHelper">The HtmlHelper object that does the rendering.</param>
        /// <param name="email">The email address whose Gravatar profile link should be rendered.</param>
        /// <returns>An HTML string of the rendered link tag.</returns>
        public static MvcHtmlString GrofileLink(this HtmlHelper htmlHelper, string linkText, string email) {
            return GrofileLink(htmlHelper, linkText, email, null);
        }

        /// <summary>
        /// Renders a script tag with the specified <paramref name="callback"/> for handling the profile JSON data.
        /// </summary>
        /// <param name="htmlHelper">The HtmlHelper object that renders the script tag.</param>
        /// <param name="email">The email of the Gravatar profile whose JSON data should be used by the <paramref name="callback"/>.</param>
        /// <param name="callback">A JavaScript function that will be called with the JSON data as a parameter when it is rendered.</param>
        /// <returns>An HTML script tag that can be included in a page.</returns>
        public static MvcHtmlString GrofileScript(this HtmlHelper htmlHelper, string email, string callback) {
            return MvcHtmlString.Create(new Grofile().RenderScript(email, callback));
        }
    }
}
