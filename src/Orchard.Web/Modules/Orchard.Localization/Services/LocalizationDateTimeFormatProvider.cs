using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;

namespace Orchard.Localization.Services {

    /// <summary>
    /// Provides an implementation of IDateTimeFormatProvider which uses Localizer to obtain
    /// property values. The strings used to resolve values through Localizer can be changed
    /// for other cultures using the normal string localization process. This is useful for
    /// adding additional or different translations than those provided by CultureInfo.
    /// </summary>
    [OrchardFeature("Orchard.Localization.DateTimeFormat")]
    [OrchardSuppressDependency("Orchard.Localization.Services.CultureDateTimeFormatProvider")]
    public class LocalizationDateTimeFormatProvider : IDateTimeFormatProvider {

        public LocalizationDateTimeFormatProvider(IOrchardServices orchardServices) {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public IEnumerable<string> MonthNames {
            get {
                return T("January, February, March, April, May, June, July, August, September, October, November, December").Text.Split(new string[] {", "}, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public IEnumerable<string> MonthNamesShort {
            get {
                return T("Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public IEnumerable<string> DayNames {
            get {
                return T("Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public IEnumerable<string> DayNamesShort {
            get {
                return T("Sun, Mon, Tue, Wed, Thu, Fri, Sat").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public IEnumerable<string> DayNamesMin {
            get {
                return T("Su, Mo, Tu, We, Th, Fr, Sa").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string ShortDateFormat {
            get {
                return T("M/d/yyyy").Text;
            }
        }

        public string ShortTimeFormat {
            get {
                return T("h:mm tt").Text;
            }
        }

        public string ShortDateTimeFormat {
            get {
                return String.Format("{0} {1}", ShortDateFormat, ShortTimeFormat);
            }
        }

        public string LongDateFormat {
            get {
                return T("dddd, MMMM d, yyyy").Text;
            }
        }

        public string LongTimeFormat {
            get {
                return T("h:mm:ss tt").Text;
            }
        }

        public string LongDateTimeFormat {
            get {
                return String.Format("{0} {1}", LongDateFormat, LongTimeFormat);
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

        public bool Use24HourTime {
            get {
                var use24HourTime = false;
                var t = T("use24HourTime: false").Text;
                var parts = t.Split(':');
                if (parts.Length == 2) {
                    Boolean.TryParse(parts[1], out use24HourTime);
                }

                return use24HourTime;
            }
        }

        public string TimeSeparator {
            get {
                return ":"; // No good way to put a colon through a colon-separated translation process...
            }
        }

        public string AmPmPrefix {
            get {
                return " "; // No good way to put a single space through a string-based translation process...
            }
        }

        public IEnumerable<string> AmPmDesignators {
            get {
                var t = T("AM;PM").Text;
                var parts = t.Split(';');
                if (parts.Length == 2) {
                    return parts;
                }

                return new string[] { "AM", "PM" };
            }
        }
    }
}