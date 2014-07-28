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
                var result = TryParseDateTime(dateTimeString, dateTimeFormat, replacements);
                if (result.HasValue) {
                    return result.Value;
                }
            }

            throw new FormatException("The string was not recognized as a valid date and time.");
        }

        public virtual DateTimeParts ParseDateTime(string dateTimeString, string format) {
            var replacements = GetDateParseReplacements().Concat(GetTimeParseReplacements()).ToDictionary(item => item.Key, item => item.Value);

            var result = TryParseDateTime(dateTimeString, format, replacements);
            if (result.HasValue) {
                return result.Value;
            }

            throw new FormatException("The string was not recognized as a valid date and time.");
        }

        public virtual DateParts ParseDate(string dateString) {
            var replacements = GetDateParseReplacements();

            foreach (var dateFormat in _dateTimeFormatProvider.AllDateFormats) {
                var result = TryParseDate(dateString, dateFormat, replacements);
                if (result.HasValue) {
                    return result.Value;
                }
            }

            throw new FormatException("The string was not recognized as a valid date.");
        }

        public virtual DateParts ParseDate(string dateString, string format) {
            var replacements = GetDateParseReplacements();

            var result = TryParseDate(dateString, format, replacements);
            if (result.HasValue) {
                return result.Value;
            }

            throw new FormatException("The string was not recognized as a valid date.");
        }

        public virtual TimeParts ParseTime(string timeString) {
            var replacements = GetTimeParseReplacements();

            foreach (var timeFormat in _dateTimeFormatProvider.AllTimeFormats) {
                var result = TryParseTime(timeString, timeFormat, replacements);
                if (result.HasValue) {
                    return result.Value;
                }
            }

            throw new FormatException("The string was not recognized as a valid time.");
        }

        public virtual TimeParts ParseTime(string timeString, string format) {
            var replacements = GetTimeParseReplacements();

            var result = TryParseTime(timeString, format, replacements);
            if (result.HasValue) {
                return result.Value;
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

        protected virtual DateTimeParts? TryParseDateTime(string dateTimeString, string format, IDictionary<string, string> replacements) {
            var dateTimePattern = ConvertFormatStringToRegexPattern(format, replacements);
            Match m = Regex.Match(dateTimeString.Trim(), dateTimePattern, RegexOptions.IgnoreCase);
            if (m.Success) {
                return new DateTimeParts(ExtractDateParts(m), ExtractTimeParts(m));
            }
            return null;
        }

        protected virtual DateParts? TryParseDate(string dateString, string format, IDictionary<string, string> replacements) {
            var datePattern = ConvertFormatStringToRegexPattern(format, replacements);
            Match m = Regex.Match(dateString.Trim(), datePattern, RegexOptions.IgnoreCase);
            if (m.Success) {
                return ExtractDateParts(m);
            }
            return null;
        }

        protected virtual TimeParts? TryParseTime(string timeString, string format, IDictionary<string, string> replacements) {
            var timePattern = ConvertFormatStringToRegexPattern(format, replacements);
            Match m = Regex.Match(timeString.Trim(), timePattern, RegexOptions.IgnoreCase);
            if (m.Success) {
                return ExtractTimeParts(m);
            }
            return null;
        }

        protected virtual DateParts ExtractDateParts(Match m) {
            int year = 0,
                month = 0,
                day = 0;

            if (m.Groups["year"].Success) {
                year = CurrentCalendar.ToFourDigitYear(Int32.Parse(m.Groups["year"].Value));
            }

            // For the month we can either use the month number, the abbreviated month name or the full month name.
            if (m.Groups["month"].Success) {
                month = Int32.Parse(m.Groups["month"].Value);
            }
            else if (m.Groups["monthNameShort"].Success) {
                var shortName = m.Groups["monthNameShort"].Value.ToLowerInvariant();
                var allShortNamesGenitive = _dateTimeFormatProvider.MonthNamesShortGenitive.Select(x => x.ToLowerInvariant()).ToList();
                var allShortNames = _dateTimeFormatProvider.MonthNamesShort.Select(x => x.ToLowerInvariant()).ToList();
                if (allShortNamesGenitive.Contains(shortName)) {
                    month = allShortNamesGenitive.IndexOf(shortName) + 1;
                }
                else if (allShortNames.Contains(shortName)) {
                    month = allShortNames.IndexOf(shortName) + 1;
                }
            }
            else if (m.Groups["monthName"].Success) {
                var name = m.Groups["monthName"].Value.ToLowerInvariant();
                var allNamesGenitive = _dateTimeFormatProvider.MonthNamesGenitive.Select(x => x.ToLowerInvariant()).ToList();
                var allNames = _dateTimeFormatProvider.MonthNames.Select(x => x.ToLowerInvariant()).ToList();
                if (allNamesGenitive.Contains(name)) {
                    month = allNamesGenitive.IndexOf(name) + 1;
                }
                else if (allNames.Contains(name)) {
                    month = allNames.IndexOf(name) + 1;
                }
            }

            if (m.Groups["day"].Success) {
                day = Int32.Parse(m.Groups["day"].Value);
            }

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
                millisecond = Int32.Parse(m.Groups["millisecond"].Value);
            }

            return new TimeParts(hour, minute, second, millisecond);
        }

        protected virtual Dictionary<string, string> GetDateParseReplacements() {
            return new Dictionary<string, string>() {       
                {"dddd", String.Format("(?<dayName>{0})", String.Join("|", _dateTimeFormatProvider.DayNames.Select(x => EscapeForRegex(x))))},
                {"ddd", String.Format("(?<dayNameShort>{0})", String.Join("|", _dateTimeFormatProvider.DayNamesShort.Select(x => EscapeForRegex(x))))},
                {"dd", "(?<day>[0-9]{2})"},
                {"d", "(?<day>[0-9]{1,2})"},
                {"MMMM", String.Format("(?<monthName>{0})", String.Join("|", _dateTimeFormatProvider.MonthNames.Union(_dateTimeFormatProvider.MonthNamesGenitive).Where(x => !String.IsNullOrEmpty(x)).Distinct().Select(x => EscapeForRegex(x))))},
                {"MMM", String.Format("(?<monthNameShort>{0})", String.Join("|", _dateTimeFormatProvider.MonthNamesShort.Union(_dateTimeFormatProvider.MonthNamesShortGenitive).Where(x => !String.IsNullOrEmpty(x)).Distinct().Select(x => EscapeForRegex(x))))},
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
                {"fffffff", "(?<millisecond>[0-9]{7})"},
                {"ffffff", "(?<millisecond>[0-9]{6})"},
                {"fffff", "(?<millisecond>[0-9]{5})"},
                {"ffff", "(?<millisecond>[0-9]{4})"},
                {"fff", "(?<millisecond>[0-9]{3})"},
                {"ff", "(?<millisecond>[0-9]{2})"},
                {"f", "(?<millisecond>[0-9]{1})"},
                {"tt", String.Format(@"\s*(?<amPm>{0}|{1})\s*", EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[0]), EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[1]))},
                {"t", String.Format(@"\s*(?<amPm>{0}|{1})\s*", EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[0]), EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[1]))},
                {" tt", String.Format(@"\s*(?<amPm>{0}|{1})\s*", EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[0]), EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[1]))},
                {" t", String.Format(@"\s*(?<amPm>{0}|{1})\s*", EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[0]), EscapeForRegex(_dateTimeFormatProvider.AmPmDesignators.ToArray()[1]))},
                {"K", @"(?<timezone>Z|(\+|-)[0-9]{2}:[0-9]{2})*"},
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
            string result = format;

            // Transform the / and : characters into culture-specific date and time separators.
            result = Regex.Replace(result, @"\/|:", m => m.Value == "/" ? _dateTimeFormatProvider.DateSeparator : _dateTimeFormatProvider.TimeSeparator);

            // Escape all characters that are intrinsic Regex syntax.
            result = EscapeForRegex(result);

            // Transform all literals to corresponding wildcard matches.
            result = Regex.Replace(result, @"(?<!\\)'(.*?)((?<!\\)')", m => String.Format("(.{{{0}}})", m.Value.Replace("\\", "").Length - 2));

            // Transform all DateTime format specifiers into corresponding Regex captures.
            result = result.ReplaceAll(replacements);

            // Make sure string is anchored to beginning and end.
            result = String.Format(@"^{0}$", result);

            return result;
        }

        protected virtual int ConvertHour12ToHour24(int hour12, bool isPm) {
            if (isPm) {
                return hour12 == 12 ? 12 : hour12 + 12;
            }
            return hour12 == 12 ? 0 : hour12;
        }

        protected virtual string EscapeForRegex(string input) {
            return Regex.Replace(input, @"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", m => String.Format(@"\{0}", m.Value));
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
