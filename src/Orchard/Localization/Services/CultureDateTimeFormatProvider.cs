using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orchard.Localization.Services {

    /// <summary>
    /// Provides an implementation of IDateTimeFormatProvider which delegates
    /// properties to the already localized strings made available by the CultureInfo
    /// object for the currently configured culture. This removes the need to have
    /// these strings translated.
    /// </summary>
    public class CultureDateTimeFormatProvider : IDateTimeFormatProvider {

        private readonly IOrchardServices _orchardServices;

        public CultureDateTimeFormatProvider(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public IEnumerable<string> MonthNames {
            get {
                return CurrentCulture.DateTimeFormat.MonthNames;
            }
        }

        public IEnumerable<string> MonthNamesShort {
            get {
                return CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
            }
        }

        public IEnumerable<string> DayNames {
            get {
                return CurrentCulture.DateTimeFormat.DayNames;
            }
        }

        public IEnumerable<string> DayNamesShort {
            get {
                return CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            }
        }

        public IEnumerable<string> DayNamesMin {
            get {
                return CurrentCulture.DateTimeFormat.ShortestDayNames;
            }
        }

        public string ShortDateFormat {
            get {
                return CurrentCulture.DateTimeFormat.ShortDatePattern;
            }
        }

        public string ShortTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.ShortTimePattern;
            }
        }

        public string ShortDateTimeFormat {
            get {
                // From empirical testing I am fairly certain this invariably evaluates to
                // the pattern actually used when printing using the 'g' (i.e. general date/time
                // pattern with short time) standard format string. /DS
                return CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('g').First();
            }
        }

        public string LongDateFormat {
            get {
                return CurrentCulture.DateTimeFormat.LongDatePattern;
            }
        }

        public string LongTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.LongTimePattern;
            }
        }

        public string LongDateTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.FullDateTimePattern;
            }
        }

        public IEnumerable<string> AllDateFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('d'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('D'));
                // The standard format strings 'M' (month/day pattern) and 'Y' (year/month
                // pattern) are excluded because they can not be round-tripped with full
                // date fidelity.
                return patterns.Distinct();
            }
        }

        public IEnumerable<string> AllTimeFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('t'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('T'));
                return patterns.Distinct();
            }
        }

        public IEnumerable<string> AllDateTimeFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('f'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('F'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('g'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('G'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('o'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('r'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('s'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('u'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('U'));
                return patterns.Distinct();
            }
        }

        public int FirstDay {
            get {
                return Convert.ToInt32(CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            }
        }

        public bool Use24HourTime {
            get {
                if (ShortTimeFormat.Contains("H")) // Capital H is the format specifier for the hour using a 24-hour clock.
                    return true;
                return false;
            }
        }

        public string DateSeparator {
            get {
                return CurrentCulture.DateTimeFormat.DateSeparator;
            }
        }

        public string TimeSeparator {
            get {
                return CurrentCulture.DateTimeFormat.TimeSeparator;
            }
        }

        public string AmPmPrefix {
            get {
                return " "; // No way to get this from CultureInfo unfortunately, so assume a single space.
            }
        }

        public IEnumerable<string> AmPmDesignators {
            get {
                return new string[] { CurrentCulture.DateTimeFormat.AMDesignator, CurrentCulture.DateTimeFormat.PMDesignator };
            }
        }

        private CultureInfo CurrentCulture {
            get {
                return CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture);
            }
        }
    }
}