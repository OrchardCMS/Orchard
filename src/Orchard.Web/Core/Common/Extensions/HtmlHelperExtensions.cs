using System;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Common.Extensions {
    public class Shapes {
        public Shapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public IHtmlString PublishedState(HtmlHelper Html, DateTime? versionPublishedUtc) {
            return Html.DateTime(versionPublishedUtc, T("Draft"));
        }

        [Shape]
        public IHtmlString PublishedWhen(dynamic Display, HtmlHelper Html, DateTime? versionPublishedUtc) {
            return Display.DateTimeRelative(dateTime: versionPublishedUtc, defaultIfNull: T("as a Draft"));
        }
    }
}
