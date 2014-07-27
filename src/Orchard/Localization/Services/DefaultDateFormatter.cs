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
            var replacements = GetDateParseReplacements().Union(GetTimeParseReplacements()).ToDictionary(item => item.Key, item => item.Value);
            var dateTimePattern = ConvertFormatStringToRegExPattern(_dateTimeFormatProvider.ShortDateTimeFormat, replacements);

            Match m = Regex.Match(dateTimeString, dateTimePattern, RegexOptions.IgnoreCase);
            if (!m.Success) {
                throw new FormatException("The string was not recognized as a valid date and time.");
            }

            return new DateTimeParts(ExtractDateParts(m), ExtractTimeParts(m));
        }

        public virtual DateParts ParseDate(string dateString) {
            var replacements = GetDateParseReplacements();
            var datePattern = ConvertFormatStringToRegExPattern(_dateTimeFormatProvider.ShortDateFormat, replacements);

            Match m = Regex.Match(dateString, datePattern, RegexOptions.IgnoreCase);
            if (!m.Success) {
                throw new FormatException("The string was not recognized as a valid date.");
            }

            return ExtractDateParts(m);
        }

        public virtual TimeParts ParseTime(string timeString) {
            var replacements = GetTimeParseReplacements();
            var timePattern = ConvertFormatStringToRegExPattern(_dateTimeFormatProvider.LongTimeFormat, replacements);

            Match m = Regex.Match(timeString, timePattern, RegexOptions.IgnoreCase);
            if (!m.Success) {
                throw new FormatException("The string was not recognized as a valid time.");
            }

            return ExtractTimeParts(m);
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
            month = Int32.Parse(m.Groups["month"].Value);
            day = Int32.Parse(m.Groups["day"].Value);

            // TODO: Also extract month names, not just numbers.

            return new DateParts(year, month, day);
        }

        protected virtual TimeParts ExtractTimeParts(Match m) {
            int hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0;

            hour = Int32.Parse(m.Groups["hour"].Value);
            minute = Int32.Parse(m.Groups["minute"].Value);
            if (m.Groups["second"].Success) {
                second = Int32.Parse(m.Groups["second"].Value);
            }
            if (m.Groups["millisecond"].Success) {
                second = Int32.Parse(m.Groups["millisecond"].Value);
            }

            // TODO: We must also handle 12-hour time with AM/PM designator.

            return new TimeParts(hour, minute, second, millisecond);
        }

        protected virtual Dictionary<string, string> GetDateParseReplacements() {
            return new Dictionary<string, string>() {       
                {"dddd", String.Format("(?<day>{0})", String.Join("|", _dateTimeFormatProvider.DayNames))},
                {"ddd", String.Format("(?<day>{0})", String.Join("|", _dateTimeFormatProvider.DayNamesShort))},
                {"dd", "(?<day>[0-9]{2})"},
                {"d", "(?<day>[0-9]{1,2})"},
                {"MMMM", String.Format("(?<month>{0})", String.Join("|", _dateTimeFormatProvider.MonthNames.Where(x => !String.IsNullOrEmpty(x))))},
                {"MMM", String.Format("(?<month>{0})", String.Join("|", _dateTimeFormatProvider.MonthNamesShort.Where(x => !String.IsNullOrEmpty(x))))},
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
                {"HH", "(?<hour>[0-9]{2})"},
                {"H", "(?<hour>[0-9]{1,2})"},
                {"hh", "(?<hour>[0-9]{2})"},
                {"h", "(?<hour>[0-9]{1,2})"},
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
                {"tt", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
                {"t", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
                {" tt", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])},
                {" t", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", _dateTimeFormatProvider.AmPmDesignators.ToArray()[0], _dateTimeFormatProvider.AmPmDesignators.ToArray()[1])}
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

        protected virtual string ConvertFormatStringToRegExPattern(string format, IDictionary<string, string> replacements) {
            string result = null;
            result = Regex.Replace(format, @"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", m => String.Format(@"\{0}", m.Value));
            result = Regex.Replace(result, @"(?<!\\)'(.*?)((?<!\\)')", m => String.Format("(.{{{0}}})", m.Value.Replace("\\", "").Length - 2));
            result = result.ReplaceAll(replacements);
            return result;
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
