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

        public virtual string[] MonthNames {
            get {
                return DateTimeFormat.MonthNames;
            }
        }

        public virtual string[] MonthNamesGenitive {
            get {
                return DateTimeFormat.MonthGenitiveNames;
            }
        }

        public virtual string[] MonthNamesShort {
            get {
                return DateTimeFormat.AbbreviatedMonthNames;
            }
        }

        public virtual string[] MonthNamesShortGenitive {
            get {
                return DateTimeFormat.AbbreviatedMonthGenitiveNames;
            }
        }

        public virtual string[] DayNames {
            get {
                return DateTimeFormat.DayNames;
            }
        }

        public virtual string[] DayNamesShort {
            get {
                return DateTimeFormat.AbbreviatedDayNames;
            }
        }

        public virtual string[] DayNamesMin {
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
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('m'));
                patterns.AddRange(DateTimeFormat.GetAllDateTimePatterns('y'));
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

        public virtual string[] AmPmDesignators {
            get {
                return new[] { DateTimeFormat.AMDesignator, DateTimeFormat.PMDesignator };
            }
        }

        public string GetEraName(int era) {
            return DateTimeFormat.GetEraName(era);
        }

        public string GetShortEraName(int era) {
            return DateTimeFormat.GetAbbreviatedEraName(era);
        }

        public int GetEra(string eraName) {
            return DateTimeFormat.GetEra(eraName);
        }

        protected virtual DateTimeFormatInfo DateTimeFormat {
            get {
                var culture = CurrentCulture;
                var calendar = CurrentCalendar;

                var usingCultureCalendar = culture.DateTimeFormat.Calendar.GetType().IsInstanceOfType(calendar);
                if (!usingCultureCalendar) {

                    // The configured calendar affects the format strings provided by the DateTimeFormatInfo
                    // class. Therefore, if the site is configured to use a calendar that is supported as an
                    // optional calendar of the configured culture, use a customized DateTimeFormatInfo instance
                    // configured with that calendar to get the correct formats.
                    foreach (var optionalCalendar in culture.OptionalCalendars) {
                        if (optionalCalendar.GetType().IsInstanceOfType(calendar)) {
                            var calendarSpecificDateTimeFormat = (DateTimeFormatInfo)culture.DateTimeFormat.Clone();
                            calendarSpecificDateTimeFormat.Calendar = optionalCalendar;
                            return calendarSpecificDateTimeFormat;
                        }
                    }

                    // If we are using a non-default calendar but it could not be found as one of the optional
                    // ones for the culture, we will explicitly check for the combination of fa-IR culture and
                    // the Persian calendar. The .NET Framework does not contain these localizations because for
                    // some strange (probably political) reason, the PersianCalendar is not one of the optional
                    // calendars for the fa-IR culture, or any other for that matter. Therefore in this case we
                    // will return an overridden DateTimeFormatInfo instance with the correct localized month
                    // names for the Persian calendar. Given that the Persian calendar is the only calendar to be
                    // "orphaned" (i.e. not supported for any culture!) in the .NET Framework, something generally
                    // considered a serious bug, I think it's justified to add this particular override
                    if (culture.Name == "fa-IR" && calendar is PersianCalendar) {
                        return PersianDateTimeFormatInfo.Build(culture.DateTimeFormat);
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