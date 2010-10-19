using System;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Common {
    public class Shapes : IDependency {
        public Shapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        [Shape]
        public IHtmlString PublishedState(HtmlHelper Html, DateTime? dateTimeUtc) {
            return Html.DateTime(dateTimeUtc, T("Draft"));
        }

        [Shape]
        public IHtmlString PublishedWhen(dynamic Display, DateTime? dateTimeUtc) {
            if (dateTimeUtc == null) {
                return T("as a Draft");
            }
            else {
                return Display.DateTimeRelative(dateTimeUtc: dateTimeUtc);
            }
        }
    }
}
