using System.Web.Mvc;
using Orchard.Core.Common.ViewModels;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Common.Extensions {
    public static class HtmlHelperExtensions {
        public static LocalizedString PublishedStateForModel(this HtmlHelper<CommonMetadataViewModel> htmlHelper, Localizer T) {
            return htmlHelper.PublishedState(htmlHelper.ViewData.Model, T);
        }

        public static LocalizedString PublishedState(this HtmlHelper htmlHelper, CommonMetadataViewModel metadata, Localizer T) {
            return htmlHelper.DateTime(metadata.VersionPublishedUtc, T("Draft"));
        }

        public static LocalizedString PublishedWhenForModel(this HtmlHelper<CommonMetadataViewModel> htmlHelper, Localizer T) {
            return htmlHelper.PublishedWhen(htmlHelper.ViewData.Model, T);
        }

        public static LocalizedString PublishedWhen(this HtmlHelper htmlHelper, CommonMetadataViewModel metadata, Localizer T) {
            return htmlHelper.DateTimeRelative(metadata.VersionPublishedUtc, T("as a Draft"), T);
        }
    }
}