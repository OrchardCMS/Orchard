using System;
using Orchard.Localization;

namespace Orchard.Localization.Services {

    public class DefaultDateTimeLocalization : IDateTimeLocalization {

        public Localizer T {
            get;
            set;
        }

        public DefaultDateTimeLocalization() {
            T = NullLocalizer.Instance;
        }

        public LocalizedString MonthNames {
            get {
                return T("January, February, March, April, May, June, July, August, September, October, November, December");
            }
        }

        public LocalizedString MonthNamesShort {
            get {
                return T("Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec");
            }
        }

        public LocalizedString DayNames {
            get {
                return T("Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday");
            }
        }

        public LocalizedString DayNamesShort {
            get {
                return T("Sun, Mon, Tue, Wed, Thu, Fri, Sat");
            }
        }

        public LocalizedString DayNamesMin {
            get {
                return T("Su, Mo, Tu, We, Th, Fr, Sa");
            }
        }

        public LocalizedString ShortDateFormat {
            get {
                return T("d");
            }
        }

        public LocalizedString ShortTimeFormat {
            get {
                return T("t");
            }
        }

        public LocalizedString ShortDateTimeFormat {
            get {
                return T("g");
            }
        }

        public LocalizedString LongDateTimeFormat {
            get {
                return T("F");
            }
        }

        public int FirstDay {
            get {
                var firstDay = 1;
                var t = T("firstDay: 1").Text;
                var parts = t.Split(':');
                if (parts.Length == 2) {
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