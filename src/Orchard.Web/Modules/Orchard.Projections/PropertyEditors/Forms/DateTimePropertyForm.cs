using System;
using System.Globalization;
using System.Web.Mvc;
using Orchard.DisplayManagement;
using Orchard.Forms.Services;
using Orchard.Localization;

namespace Orchard.Projections.PropertyEditors.Forms {
    public class DateTimePropertyForm : IFormProvider {

        public const string FormName = "DateTimeProperty";

        protected dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public DateTimePropertyForm(IShapeFactory shapeFactory) {
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            Func<IShapeFactory, object> form =
                shape => {

                    var f = Shape.Form(
                        _Format: Shape.SelectList(
                            Id: "format", Name: "Format",
                            Title: T("Date format"),
                            Size: 1,
                            Multiple: false
                        )
                    );

                    f._Format.Add(new SelectListItem { Value = "d", Text = T("Short date pattern: 6/15/2009").Text });
                    f._Format.Add(new SelectListItem { Value = "D", Text = T("Long date pattern: Monday, June 15, 2009").Text });
                    f._Format.Add(new SelectListItem { Value = "f", Text = T("Full date/time pattern (short time): Monday, June 15, 2009 1:45 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "F", Text = T("Full date/time pattern (long time): Monday, June 15, 2009 1:45:30 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "g", Text = T("General date/time pattern (short time): 6/15/2009 1:45 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "G", Text = T("General date/time pattern (long time): 6/15/2009 1:45:30 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "M", Text = T("Month/day pattern: June 15").Text });
                    f._Format.Add(new SelectListItem { Value = "O", Text = T("Round-trip date/time pattern: 2009-06-15T13:45:30.0900000").Text });
                    f._Format.Add(new SelectListItem { Value = "R", Text = T("RFC1123 pattern: Mon, 15 Jun 2009 20:45:30 GMT").Text });
                    f._Format.Add(new SelectListItem { Value = "s", Text = T("Sortable date/time pattern: 2009-06-15T13:45:30").Text });
                    f._Format.Add(new SelectListItem { Value = "t", Text = T("Long time pattern: 1:45 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "T", Text = T("Long time pattern: 1:45:30 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "u", Text = T("Universal sortable date/time pattern: 2009-06-15 20:45:30Z").Text });
                    f._Format.Add(new SelectListItem { Value = "U", Text = T("Universal full date/time pattern:  Monday, June 15, 2009 8:45:30 PM").Text });
                    f._Format.Add(new SelectListItem { Value = "Y", Text = T("Year month pattern: June, 2009").Text });
                    f._Format.Add(new SelectListItem { Value = "day", Text = T("Day: 15").Text });
                    f._Format.Add(new SelectListItem { Value = "month", Text = T("Month: 6").Text });
                    f._Format.Add(new SelectListItem { Value = "year", Text = T("Year: 2009").Text });
                    f._Format.Add(new SelectListItem { Value = "dayOfYear", Text = T("Day of year: 166").Text });
                    f._Format.Add(new SelectListItem { Value = "ago", Text = T("Time ago").Text });

                    return f;
                };

            context.Form(FormName, form);

        }
        
        public static dynamic FormatDateTime(dynamic display, DateTime dateTime, dynamic state, string culture) {

            string format = state.Format;
            var cultureInfo = CultureInfo.CreateSpecificCulture(culture);

            switch(format) {
                case "ago":
                    return display.DateTimeRelative(dateTimeUtc: dateTime);
                case "day" :
                    return dateTime.Day.ToString(cultureInfo);
                case "month" :
                    return dateTime.Month.ToString(cultureInfo);
                case "year" :
                    return dateTime.Year.ToString(cultureInfo);
                case "dayOfYear":
                    return dateTime.DayOfYear.ToString(cultureInfo);
                default:
                    return dateTime.ToString(format, cultureInfo);
            }
        }
    }
}