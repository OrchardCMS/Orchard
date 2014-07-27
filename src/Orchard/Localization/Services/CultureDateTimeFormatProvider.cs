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

        // TODO: This implementation should probably also depend on the current calendar, because DateTimeFormatInfo returns different strings depending on the calendar.

        private readonly IWorkContextAccessor _workContextAccessor;

        public CultureDateTimeFormatProvider(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public virtual IEnumerable<string> MonthNames {
            get {
                return CurrentCulture.DateTimeFormat.MonthNames;
            }
        }

        public virtual IEnumerable<string> MonthNamesShort {
            get {
                return CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
            }
        }

        public virtual IEnumerable<string> DayNames {
            get {
                return CurrentCulture.DateTimeFormat.DayNames;
            }
        }

        public virtual IEnumerable<string> DayNamesShort {
            get {
                return CurrentCulture.DateTimeFormat.AbbreviatedDayNames;
            }
        }

        public virtual IEnumerable<string> DayNamesMin {
            get {
                return CurrentCulture.DateTimeFormat.ShortestDayNames;
            }
        }

        public virtual string ShortDateFormat {
            get {
                return CurrentCulture.DateTimeFormat.ShortDatePattern;
            }
        }

        public virtual string ShortTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.ShortTimePattern;
            }
        }

        public virtual string ShortDateTimeFormat {
            get {
                // From empirical testing I am fairly certain this invariably evaluates to
                // the pattern actually used when printing using the 'g' (i.e. general date/time
                // pattern with short time) standard format string. /DS
                return CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('g').First();
            }
        }

        public virtual string LongDateFormat {
            get {
                return CurrentCulture.DateTimeFormat.LongDatePattern;
            }
        }

        public virtual string LongTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.LongTimePattern;
            }
        }

        public virtual string LongDateTimeFormat {
            get {
                return CurrentCulture.DateTimeFormat.FullDateTimePattern;
            }
        }

        public virtual IEnumerable<string> AllDateFormats {
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

        public virtual IEnumerable<string> AllTimeFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('t'));
                patterns.AddRange(CurrentCulture.DateTimeFormat.GetAllDateTimePatterns('T'));
                return patterns.Distinct();
            }
        }

        public virtual IEnumerable<string> AllDateTimeFormats {
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

        public virtual int FirstDay {
            get {
                return Convert.ToInt32(CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            }
        }

        public virtual bool Use24HourTime {
            get {
                if (ShortTimeFormat.Contains("H")) // Capital H is the format specifier for the hour using a 24-hour clock.
                    return true;
                return false;
            }
        }

        public virtual string DateSeparator {
            get {
                return CurrentCulture.DateTimeFormat.DateSeparator;
            }
        }

        public virtual string TimeSeparator {
            get {
                return CurrentCulture.DateTimeFormat.TimeSeparator;
            }
        }

        public virtual string AmPmPrefix {
            get {
                return " "; // No way to get this from CultureInfo unfortunately, so assume a single space.
            }
        }

        public virtual IEnumerable<string> AmPmDesignators {
            get {
                return new string[] { CurrentCulture.DateTimeFormat.AMDesignator, CurrentCulture.DateTimeFormat.PMDesignator };
            }
        }

        protected virtual CultureInfo CurrentCulture {
            get {
                var workContext = _workContextAccessor.GetContext();
                return CultureInfo.GetCultureInfo(workContext.CurrentCulture);
            }
        }
    }
}