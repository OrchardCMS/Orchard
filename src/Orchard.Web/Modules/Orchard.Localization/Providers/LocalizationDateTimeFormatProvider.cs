﻿using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Localization.Services;

namespace Orchard.Localization.Providers {

    /// <summary>
    /// Provides an implementation of IDateTimeFormatProvider which uses Localizer to obtain
    /// property values. The strings used to resolve values through Localizer can be changed
    /// for other cultures using the normal string localization process. This is useful for
    /// adding additional or different translations than those provided by CultureInfo.
    /// </summary>
    [OrchardFeature("Orchard.Localization.DateTimeFormat")]
    [OrchardSuppressDependency("Orchard.Localization.Services.CultureDateTimeFormatProvider")]
    public class LocalizationDateTimeFormatProvider : IDateTimeFormatProvider {

        public LocalizationDateTimeFormatProvider() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string[] MonthNames {
            get {
                return T("January, February, March, April, May, June, July, August, September, October, November, December").Text.Split(new string[] {", "}, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public virtual string[] MonthNamesGenitive {
            get {
                return MonthNames;
            }
        }

        public string[] MonthNamesShort {
            get {
                return T("Jan, Feb, Mar, Apr, May, Jun, Jul, Aug, Sep, Oct, Nov, Dec").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public virtual string[] MonthNamesShortGenitive {
            get {
                return MonthNamesShort;
            }
        }

        public string[] DayNames {
            get {
                return T("Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string[] DayNamesShort {
            get {
                return T("Sun, Mon, Tue, Wed, Thu, Fri, Sat").Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public string[] DayNamesMin {
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
                return T("M/d/yyyy h:mm tt").Text;
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
                return T("dddd, MMMM d, yyyy h:mm:ss tt").Text;
            }
        }

        public IEnumerable<string> AllDateFormats {
            get {
                return new[] { ShortDateFormat, LongDateFormat };
            }
        }

        public IEnumerable<string> AllTimeFormats {
            get {
                return new[] { ShortTimeFormat, LongTimeFormat };
            }
        }

        public IEnumerable<string> AllDateTimeFormats {
            get {
                return new[] { ShortDateTimeFormat, LongDateTimeFormat };
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

        public string DateSeparator {
            get {
                return "/"; // Since we can't do it with TimeSeparator why do it with this one...
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

        public string[] AmPmDesignators {
            get {
                var t = T("AM;PM").Text;
                var parts = t.Split(';');
                if (parts.Length == 2) {
                    return parts;
                }

                return new string[] { "AM", "PM" };
            }
        }

        public string GetEraName(int era) {
            var t = T("A.D.;A.D.").Text;
            var parts = t.Split(';');
            if (parts.Length >= era + 1) {
                return parts[era];
            }

            return null;
        }

        public string GetShortEraName(int era) {
            var t = T("AD;AD").Text;
            var parts = t.Split(';');
            if (parts.Length >= era + 1) {
                return parts[era];
            }

            return null;
        }

        public int GetEra(string eraName) {
            var t = T("AD;AD").Text;
            var parts = t.ToLowerInvariant().Split(';');
            if (parts.Contains(eraName.ToLowerInvariant())) {
                return parts.ToList().IndexOf(eraName.ToLowerInvariant());
            }

            throw new ArgumentOutOfRangeException("eraName");
        }
    }
}