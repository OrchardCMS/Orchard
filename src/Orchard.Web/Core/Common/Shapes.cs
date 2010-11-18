using System;
using System.Web;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Localization;
using Orchard.Mvc.Html;

namespace Orchard.Core.Common {
    public class Shapes : IShapeTableProvider {
        public Shapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Body_Editor")
                .OnDisplaying(displaying => {
                    string flavor = displaying.Shape.EditorFlavor;
                    displaying.ShapeMetadata.Alternates.Add("Body_Editor__" + flavor);
                });
            builder.Describe("Fields_Common_Text")
                .OnDisplaying(displaying => {
                    string textFieldName = displaying.Shape.Name;
                    displaying.ShapeMetadata.Alternates.Add("Fields_Common_Text__" + textFieldName);
                });
        }

        [Shape]
        public IHtmlString PublishedState(HtmlHelper Html, DateTime? dateTimeUtc) {
            return Html.DateTime(dateTimeUtc, T("Draft"));
        }

        [Shape]
        public IHtmlString PublishedWhen(dynamic Display, DateTime? dateTimeUtc) {
            if (dateTimeUtc == null)
                return T("as a Draft");

            return Display.DateTimeRelative(dateTimeUtc: dateTimeUtc);
        }
    }
}
