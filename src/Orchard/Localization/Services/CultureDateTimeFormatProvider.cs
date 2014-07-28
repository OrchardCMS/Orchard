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

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICalendarManager _calendarManager;

        public CultureDateTimeFormatProvider(
            IWorkContextAccessor workContextAccessor,
            ICalendarManager calendarManager) {
            _workContextAccessor = workContextAccessor;
            _calendarManager = calendarManager;
        }

        public virtual IEnumerable<string> MonthNames {
            get {
                return DateTimeFormat.MonthNames;
            }
        }

        public virtual IEnumerable<string> MonthNamesGenitive {
            get {
                return DateTimeFormat.MonthGenitiveNames;
            }
        }

        public virtual IEnumerable<string> MonthNamesShort {
            get {
                return DateTimeFormat.AbbreviatedMonthNames;
            }
        }

        public virtual IEnumerable<string> MonthNamesShortGenitive {
            get {
                return DateTimeFormat.AbbreviatedMonthGenitiveNames;
            }
        }

        public virtual IEnumerable<string> DayNames {
            get {
                return DateTimeFormat.DayNames;
            }
        }

        public virtual IEnumerable<string> DayNamesShort {
            get {
                return DateTimeFormat.AbbreviatedDayNames;
            }
        }

        public virtual IEnumerable<string> DayNamesMin {
            get {
                return DateTimeFormat.ShortestDayNames;
            }
        }

        public virtual string ShortDateFormat {
            get {
                return DateTimeFormat.ShortDatePattern;
            }
        }

        public virtual string ShortTimeFormat {
            get {
                return DateTimeFormat.ShortTimePattern;
            }
        }

        public virtual string ShortDateTimeFormat {
            get {
                // From empirical testing I am fairly certain First() invariably evaluates to
                // the pattern actually used when printing using the 'g' (i.e. general date/time
                // pattern with short time) standard format string. /DS
                return DateTimeFormat.GetAllDateTimePatterns('g').First();
            }
        }

        public virtual string LongDateFormat {
            get {
                return DateTimeFormat.LongDatePattern;
            }
        }

        public virtual string LongTimeFormat {
            get {
                return DateTimeFormat.LongTimePattern;
            }
        }

        public virtual string LongDateTimeFormat {
            get {
                return DateTimeFormat.FullDateTimePattern;
            }
        }

        public virtual IEnumerable<string> AllDateFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('d'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('D'));
                // The standard format strings 'M' (month/day pattern) and 'Y' (year/month
                // pattern) are excluded because they can not be round-tripped with full
                // date fidelity.
                return patterns.Distinct();
            }
        }

        public virtual IEnumerable<string> AllTimeFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('t'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('T'));
                return patterns.Distinct();
            }
        }

        public virtual IEnumerable<string> AllDateTimeFormats {
            get {
                var patterns = new List<string>();
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('f'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('F'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('g'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('G'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('o'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('r'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('s'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('u'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('U'));
                return patterns.Distinct();
            }
        }

        public virtual int FirstDay {
            get {
                return Convert.ToInt32(DateTimeFormat.FirstDayOfWeek);
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
                return DateTimeFormat.DateSeparator;
            }
        }

        public virtual string TimeSeparator {
            get {
                return DateTimeFormat.TimeSeparator;
            }
        }

        public virtual string AmPmPrefix {
            get {
                return " "; // No way to get this from CultureInfo unfortunately, so assume a single space.
            }
        }

        public virtual IEnumerable<string> AmPmDesignators {
            get {
                return new string[] { DateTimeFormat.AMDesignator, DateTimeFormat.PMDesignator };
            }
        }

        protected virtual DateTimeFormatInfo DateTimeFormat {
            get {
                var culture = CurrentCulture;
                var calendar = CurrentCalendar;

                // The configured Calendar affects the format strings provided by the DateTimeFormatInfo
                // class. Therefore, if the site is configured to use a calendar that is supported as an
                // optional calendar of the configured culture, use a customized DateTimeFormatInfo instance
                // configured with that calendar to get the correct formats.
                var usingCultureCalendar = culture.DateTimeFormat.Calendar.GetType().IsInstanceOfType(calendar);
                if (!usingCultureCalendar) {
                    foreach (var optionalCalendar in culture.OptionalCalendars) {
                        if (optionalCalendar.GetType().IsInstanceOfType(calendar)) {
                            var calendarSpecificDateTimeFormat = (DateTimeFormatInfo)culture.DateTimeFormat.Clone();
                            calendarSpecificDateTimeFormat.Calendar = optionalCalendar;
                            return calendarSpecificDateTimeFormat;
                        }
                    }
                }

                return culture.DateTimeFormat;
            }
        }

        protected virtual CultureInfo CurrentCulture {
            get {
                var workContext = _workContextAccessor.GetContext();
                return CultureInfo.GetCultureInfo(workContext.CurrentCulture);
            }
        }

        protected virtual Calendar CurrentCalendar {
            get {
                var workContext = _workContextAccessor.GetContext();
                if (!String.IsNullOrEmpty(workContext.CurrentCalendar))
                    return _calendarManager.GetCalendarByName(workContext.CurrentCalendar);
                return CurrentCulture.Calendar;
            }
        }
    }
}