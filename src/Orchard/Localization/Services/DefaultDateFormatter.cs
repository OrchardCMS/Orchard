using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Framework.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Framework.Localization.Services {

    public class DefaultDateFormatter : IDateFormatter {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IDateTimeFormatProvider _dateTimeFormatProvider;
        private readonly ICalendarManager _calendarManager;

        public DefaultDateFormatter(
            IWorkContextAccessor workContextAccessor,
            IDateTimeFormatProvider dateTimeFormatProvider,
            ICalendarManager calendarManager) {
            _workContextAccessor = workContextAccessor;
            _dateTimeFormatProvider = dateTimeFormatProvider;
            _calendarManager = calendarManager;
        }

        public virtual DateTimeParts ParseDateTime(string dateTimeString) {
            var replacements = GetDateParseReplacements().Concat(GetTimeParseReplacements()).ToDictionary(item => item.Key, item => item.Value);

            foreach (var dateTimeFormat in _dateTimeFormatProvider.AllDateTimeFormats) {
                var dateTimePattern = ConvertFormatStringToRegexPattern(dateTimeFormat, replacements);
                Match m = Regex.Match(dateTimeString.Trim(), dateTimePattern, RegexOptions.IgnoreCase);
                if (m.Success) {
                    return new DateTimeParts(ExtractDateParts(m), ExtractTimeParts(m));
                }
            }

            throw new FormatException("The string was not recognized as a valid date and time.");
        }

        public virtual DateParts ParseDate(string dateString) {
            var replacements = GetDateParseReplacements();

            foreach (var dateFormat in _dateTimeFormatProvider.AllDateFormats) {
                var datePattern = ConvertFormatStringToRegexPattern(dateFormat, replacements);
                Match m = Regex.Match(dateString.Trim(), datePattern, RegexOptions.IgnoreCase);
                if (m.Success) {
                    return ExtractDateParts(m);
                }
            }

            throw new FormatException("The string was not recognized as a valid date.");
        }

        public virtual TimeParts ParseTime(string timeString) {
            var replacements = GetTimeParseReplacements();

            foreach (var timeFormat in _dateTimeFormatProvider.AllTimeFormats) {
                var timePattern = ConvertFormatStringToRegexPattern(timeFormat, replacements);
                Match m = Regex.Match(timeString.Trim(), timePattern, RegexOptions.IgnoreCase);
                if (m.Success) {
                    return ExtractTimeParts(m);
                }
            }

            throw new FormatException("The string was not recognized as a valid time.");
        }

        public virtual string FormatDateTime(DateTimeParts parts) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        public virtual string FormatDateTime(DateTimeParts parts, string format) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        public virtual string FormatDate(DateParts parts) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        public virtual string FormatDate(DateParts parts, string format) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        public virtual string FormatTime(TimeParts parts) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        public virtual string FormatTime(TimeParts parts, string format) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        protected virtual DateParts ExtractDateParts(Match m) {
            int year = 0,
                month = 0,
                day = 0;

            year = CurrentCalendar.ToFourDigitYear(Int32.Parse(m.Groups["year"].Value));

            // For the month we can either use the month number, the abbreviated month name or the full month name.
            if (m.Groups["month"].Success) {
                month = Int32.Parse(m.Groups["month"].Value);
            }
            else if (m.Groups["monthNameShort"].Success) {
                month = _dateTimeFormatProvider.MonthNamesShort.Select(x => x.ToLowerInvariant()).ToList().IndexOf(m.Groups["monthNameShort"].Value.ToLowerInvariant()) + 1;
            }
            else if (m.Groups["monthName"].Success) {
                month = _dateTimeFormatProvider.MonthNames.Select(x => x.ToLowerInvariant()).ToList().IndexOf(m.Groups["monthName"].Value.ToLowerInvariant()) + 1;
            }

            day = Int32.Parse(m.Groups["day"].Value);

            return new DateParts(year, month, day);
        }

        protected virtual TimeParts ExtractTimeParts(Match m) {
            int hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0;

            // For the hour we can either use 24-hour notation or 12-hour notation in combination with AM/PM designator.
            if (m.Groups["hour24"].Success) {
                hour = Int32.Parse(m.Groups["hour24"].Value);
            }
            else if (m.Groups["hour12"].Success) {
                if (!m.Groups["amPm"].Success) {
                    throw new FormatException("The string was not recognized as a valid time. The hour is in 12-hour notation but no AM/PM designator was found.");
                }
                var isPm = m.Groups["amPm"].Value.Equals(_dateTimeFormatProvider.AmPmDesignators.ToArray()[1], StringComparison.InvariantCultureIgnoreCase);
                hour = ConvertHour12ToHour24(Int32.Parse(m.Groups["hour12"].Value), isPm);
            }

            if (m.Groups["minute"].Success) {
                minute = Int32.Parse(m.Groups["minute"].Value);
            }

            if (m.Groups["second"].Success) {
                second = Int32.Parse(m.Groups["second"].Value);
            }
            
            if (m.Groups["millisecond"].Success) {
                second = Int32.Parse(m.Groups["millisecond"].Value);
            }

            return new TimeParts(hour, minute, second, millisecond);
        }

        protected virtual Dictionary<string, string> GetDateParseReplacements() {
            return new Dictionary<string, string>() {       
                {"dddd", String.Format("(?<dayName>{0})", String.Join("|", _dateTimeFormatProvider.DayNames))},
                {"ddd", String.Format("(?<dayNameShort>{0})", String.Join("|", _dateTimeFormatProvider.DayNamesShort))},
                {"dd", "(?<day>[0-9]{2})"},
                {"d", "(?<day>[0-9]{1,2})"},
                {"MMMM", String.Format("(?<monthName>{0})", String.Join("|", _dateTimeFormatProvider.MonthNames.Where(x => !String.IsNullOrEmpty(x))))},
                {"MMM", String.Format("(?<monthNameShort>{0})", String.Join("|", _dateTimeFormatProvider.MonthNamesShort.Where(x => !String.IsNullOrEmpty(x))))},
                {"MM", "(?<month>[0-9]{2})"},
                {"M", "(?<month>[0-9]{1,2})"},
                {"yyyyy", "(?<year>[0-9]{5})"},
                {"yyyy", "(?<year>[0-9]{4})"},
                {"yyy", "(?<year>[0-9]{3})"},
                {"yy", "(?<year>[0-9]{2})"}, 
                {"y", "(?<year>[0-9]{1})"}
            };
        }

        protected virtual Dictionary<string, string> GetTimeParseReplacements() {
            return new Dictionary<string, string>() {       
                {"HH", "(?<hour24>[0-9]{2})"},
                {"H", "(?<hour24>[0-9]{1,2})"},
                {"hh", "(?<hour12>[0-9]{2})"},
                {"h", "(?<hour12>[0-9]{1,2})"},
                {"mm", "(?<minute>[0-9]{2})"},
                {"m", "(?<minute>[0-9]{1,2})"},
                {"ss", "(?<second>[0-9]{2})"},      
                {"s", "(?<second>[0-9]{1,2})"},
                {"f", "(?<millisecond>[0-9]{1})"},
                {"ff", "(?<millisecond>[0-9]{2})"},
                {"fff", "(?<millisecond>[0-9]{3})"},
                {"ffff", "(?<millisecond>[0-9]{4})"},
                {"fffff", "(?<millisecond>[0-9]{5})"},
                {"ffffff", "(?<millisecond>[0-9]{6})"},
                {"tt", String.Format("\\s*(?<amPm>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
                {"t", String.Format("\\s*(?<amPm>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
                {" tt", String.Format("\\s*(?<amPm>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
                {" t", String.Format("\\s*(?<amPm>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])}
            };
        }

        //protected virtual Dictionary<string, string> GetDateFormatReplacements() {
        //    return new Dictionary<string, string>() {       
        //        {"dddd", "{5:dddd}"},
        //        {"ddd", "{6:ddd}"},
        //        {"dd", "{2:00}"},
        //        {"d", "{2:##}"},
        //        {"MMMM", "{3:MMMM}"},
        //        {"MMM", "{4:MMM}"},
        //        {"MM", "{1:00}"},
        //        {"M", "{1:##}"},
        //        {"yyyyy", "{1:00000}"},
        //        {"yyyy", "{1:0000}"},
        //        {"yyy", "{1:000}"},
        //        {"yy", "{1:00}"}, 
        //        {"y", "{1:0}"}
        //    };
        //}

        //protected virtual Dictionary<string, string> GetTimeFormatReplacements() {
        //    return new Dictionary<string, string>() {       
        //        {"HH", "(?<hour>[0-9]{2})"},
        //        {"H", "(?<hour>[0-9]{1,2})"},
        //        {"hh", "(?<hour>[0-9]{2})"},
        //        {"h", "(?<hour>[0-9]{1,2})"},
        //        {"mm", "(?<minute>[0-9]{2})"},
        //        {"m", "(?<minute>[0-9]{1,2})"},
        //        {"ss", "(?<second>[0-9]{2})"},      
        //        {"s", "(?<second>[0-9]{1,2})"},
        //        {"f", "(?<millisecond>[0-9]{1})"},
        //        {"ff", "(?<millisecond>[0-9]{2})"},
        //        {"fff", "(?<millisecond>[0-9]{3})"},
        //        {"ffff", "(?<millisecond>[0-9]{4})"},
        //        {"fffff", "(?<millisecond>[0-9]{5})"},
        //        {"ffffff", "(?<millisecond>[0-9]{6})"},
        //        {"tt", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
        //        {"t", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
        //        {" tt", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
        //        {" t", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])}
        //    };
        //}

        protected virtual string ConvertFormatStringToRegexPattern(string format, IDictionary<string, string> replacements) {
            string result = null;
            result = Regex.Replace(format, @"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", m => String.Format(@"\{0}", m.Value));
            result = Regex.Replace(result, @"(?<!\\)'(.*?)((?<!\\)')", m => String.Format("(.{{{0}}})", m.Value.Replace("\\", "").Length - 2));
            result = result.ReplaceAll(replacements);
            result = String.Format(@"^{0}$", result); // Make sure string is anchored to beginning and end.
            return result;
        }

        protected virtual int ConvertHour12ToHour24(int hour12, bool isPm) {
            if (isPm) {
                return hour12 == 12 ? 12 : hour12 + 12;
            }
            return hour12 == 12 ? 0 : hour12;
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
