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
        }

        [Shape]
        public IHtmlString PublishedState(dynamic Display, DateTime createdDateTimeUtc, DateTime? publisheddateTimeUtc, LocalizedString customDateFormat) {
            if (!publisheddateTimeUtc.HasValue) {
                return T("Draft");
            }

            return Display.DateTime(DateTimeUtc: createdDateTimeUtc, CustomFormat: customDateFormat);
        }

        [Shape]
        public IHtmlString PublishedWhen(dynamic Display, DateTime? dateTimeUtc) {
            if (dateTimeUtc == null)
                return T("as a Draft");

            return Display.DateTimeRelative(DateTimeUtc: dateTimeUtc);
        }
    }
}
