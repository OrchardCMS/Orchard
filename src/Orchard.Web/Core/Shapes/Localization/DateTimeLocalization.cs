using System;
using Orchard.Localization;

namespace Orchard.Core.Shapes.Localization {

    public interface IDateTimeLocalization : IDependency {
        LocalizedString MonthNames { get; }
        LocalizedString MonthNamesShort { get; }
        LocalizedString DayNames { get; }
        LocalizedString DayNamesShort { get; }
        LocalizedString DayNamesMin { get; }

        LocalizedString ShortDateFormat { get; }
        LocalizedString ShortTimeFormat { get; }
        LocalizedString LongDateTimeFormat { get; }

        /// <summary>
        /// The first day of the week, Sun = 0, Mon = 1, ...
        /// </summary>
        int FirstDay { get; }

        /// <summary>
        /// True if the year select precedes month, false for month then year
        /// </summary>
        bool ShowMonthAfterYear { get; }

        /// <summary>
        /// Additional text to append to the year in the month headers
        /// </summary>
        string YearSuffix { get; }
    }

    public class DateTimeLocalization : IDateTimeLocalization {

        public Localizer T { get; set; }

        public DateTimeLocalization() {
            T = NullLocalizer.Instance;
        }

        public LocalizedString MonthNames {
            get { return T("January, February, March, April, May, June, July, August, September, October, November, December"); }
        }

        public LocalizedString MonthNamesShort {
            get { return T("Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec"); }
        }

        public LocalizedString DayNames {
            get { return T("Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday"); }
        }

        public LocalizedString DayNamesShort {
            get { return T("Sun, Mon, Tue, Wed, Thu, Fri, Sat"); }
        }

        public LocalizedString DayNamesMin {
            get { return T("Su, Mo, Tu, We, Th, Fr, Sa"); }
        }

        public LocalizedString ShortDateFormat {
            get { return T("M/d/yyyy"); }
        }

        public LocalizedString ShortTimeFormat {
            get { return T("h:mm tt"); }
        }

        public LocalizedString LongDateTimeFormat {
            get { return T("MMM d yyyy h:mm tt"); }
        }

        public int FirstDay {
            get {
                var firstDay = 1;
                var t = T("firstDay: 1").Text;
                var parts = t.Split(':');
                if(parts.Length == 2) {
                    Int32.TryParse(parts[1], out firstDay);
                }

                return firstDay;
            }
        }

        public bool ShowMonthAfterYear {
            get {
                var showMonthAfterYear = false;
                var t = T("showMonthAfterYear: false").Text;
                var parts = t.Split(':');
                if (parts.Length == 2) {
                    Boolean.TryParse(parts[1], out showMonthAfterYear);
                }

                return showMonthAfterYear;
            }
        }

        public string YearSuffix {
            get {
                var yearSuffix = String.Empty;
                var t = T("yearSuffix: ").Text;
                var parts = t.Split(':');
                if (parts.Length == 2) {
                    return parts[1].Trim();
                }

                return yearSuffix;
            }
        }
    }
}