using System;
using System.Web.Mvc;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Common.Extensions {
    public static class HtmlHelperExtensions {
        public static LocalizedString PublishedState(this HtmlHelper htmlHelper, DateTime? versionPublishedUtc, Localizer T) {
            return htmlHelper.DateTime(versionPublishedUtc, T("Draft"));
        }
        public static LocalizedString PublishedWhen(this HtmlHelper htmlHelper, DateTime? versionPublishedUtc, Localizer T) {
            return htmlHelper.DateTimeRelative(versionPublishedUtc, T("as a Draft"), T);
        }
    }
}