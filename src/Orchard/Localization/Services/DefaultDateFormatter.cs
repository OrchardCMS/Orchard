using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Utility.Extensions;

namespace Orchard.Localization.Services {

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
            return FormatDateTime(parts, _dateTimeFormatProvider.ShortDateTimeFormat);
        }

        public virtual string FormatDateTime(DateTimeParts parts, string format) {
            var useMonthNameGenitive = GetUseGenitiveMonthName(format);

            var replacements = GetDateFormatReplacements(useMonthNameGenitive).Concat(GetTimeFormatReplacements()).ToDictionary(item => item.Key, item => item.Value);
            var formatString = ConvertToFormatString(format, replacements);
            var calendar = CurrentCalendar;

            int twoDigitYear, hour12, offsetHours, offsetMinutes;
            bool isPm;
            string monthName, monthNameShort, monthNameGenitive, monthNameShortGenitive, dayName, dayNameShort, eraName, eraNameShort, amPm, amPmShort, timeZone, offsetSign, fraction1Zero, fraction2Zero, fraction3Zero, fraction1Digit, fraction2Digit, fraction3Digit;
            GetDateFormatValues(parts.Date, calendar, out twoDigitYear, out monthName, out monthNameShort, out monthNameGenitive, out monthNameShortGenitive, out dayName, out dayNameShort, out eraName, out eraNameShort);
            GetTimeFormatValues(parts.Time, out isPm, out hour12, out amPm, out amPmShort, out timeZone, out offsetSign, out offsetHours, out offsetMinutes, out fraction1Zero, out fraction2Zero, out fraction3Zero, out fraction1Digit, out fraction2Digit, out fraction3Digit);

            return String.Format(formatString, parts.Date.Year, twoDigitYear, parts.Date.Month, monthName, monthNameShort, monthNameGenitive, monthNameShortGenitive, parts.Date.Day, dayName, dayNameShort, eraName, eraNameShort, parts.Time.Hour, hour12, parts.Time.Minute, parts.Time.Second, fraction1Zero, fraction2Zero, fraction3Zero, fraction1Digit, fraction2Digit, fraction3Digit, amPm, amPmShort, timeZone, offsetSign, offsetHours, offsetMinutes);
        }

        public virtual string FormatDate(DateParts parts) {
            return FormatDate(parts, _dateTimeFormatProvider.ShortDateFormat);
        }

        public virtual string FormatDate(DateParts parts, string format) {
            var useMonthNameGenitive = GetUseGenitiveMonthName(format);

            var replacements = GetDateFormatReplacements(useMonthNameGenitive);
            var formatString = ConvertToFormatString(format, replacements);
            var calendar = CurrentCalendar;

            int twoDigitYear;
            string monthName, monthNameShort, monthNameGenitive, monthNameShortGenitive, dayName, dayNameShort, eraName, eraNameShort;
            GetDateFormatValues(parts, calendar, out twoDigitYear, out monthName, out monthNameShort, out monthNameGenitive, out monthNameShortGenitive, out dayName, out dayNameShort, out eraName, out eraNameShort);
            
            return String.Format(formatString, parts.Year, twoDigitYear, parts.Month, monthName, monthNameShort, monthNameGenitive, monthNameShortGenitive, parts.Day, dayName, dayNameShort, eraName, eraNameShort);
        }

        public virtual string FormatTime(TimeParts parts) {
            return FormatTime(parts, _dateTimeFormatProvider.LongTimeFormat);
        }

        public virtual string FormatTime(TimeParts parts, string format) {
            var replacements = GetTimeFormatReplacements();
            var formatString = ConvertToFormatString(format, replacements);

            bool isPm;
            int hour12, offsetHours, offsetMinutes;
            string amPm, amPmShort, timeZone, offsetSign, fraction1Zero, fraction2Zero, fraction3Zero, fraction1Digit, fraction2Digit, fraction3Digit;
            GetTimeFormatValues(parts, out isPm, out hour12, out amPm, out amPmShort, out timeZone, out offsetSign, out offsetHours, out offsetMinutes, out fraction1Zero, out fraction2Zero, out fraction3Zero, out fraction1Digit, out fraction2Digit, out fraction3Digit);

            return String.Format(formatString, null, null, null, null, null, null, null, null, null, null, null, null, parts.Hour, hour12, parts.Minute, parts.Second, fraction1Zero, fraction2Zero, fraction3Zero, fraction1Digit, fraction2Digit, fraction3Digit, amPm, amPmShort, timeZone, offsetSign, offsetHours, offsetMinutes);
        }

        protected virtual DateTimeParts? TryParseDateTime(string dateTimeString, string format, IDictionary<string, string> replacements) {
            var dateTimePattern = ConvertToRegexPattern(format, replacements);
            Match m = Regex.Match(dateTimeString, dateTimePattern, RegexOptions.IgnoreCase);
            if (m.Success) {
                return new DateTimeParts(ExtractDateParts(m), ExtractTimeParts(m));
            }
            return null;
        }

        protected virtual DateParts? TryParseDate(string dateString, string format, IDictionary<string, string> replacements) {
            var datePattern = ConvertToRegexPattern(format, replacements);
            Match m = Regex.Match(dateString, datePattern, RegexOptions.IgnoreCase);
            if (m.Success) {
                return ExtractDateParts(m);
            }
            return null;
        }

        protected virtual TimeParts? TryParseTime(string timeString, string format, IDictionary<string, string> replacements) {
            var timePattern = ConvertToRegexPattern(format, replacements);
            Match m = Regex.Match(timeString, timePattern, RegexOptions.IgnoreCase);
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

            var kind = DateTimeKind.Unspecified;
            var offset = TimeSpan.Zero;

            // For the hour we can either use 24-hour notation or 12-hour notation in combination with AM/PM designator.
            if (m.Groups["hour24"].Success) {
                hour = Int32.Parse(m.Groups["hour24"].Value);
            }
            else if (m.Groups["hour12"].Success) {
                if (!m.Groups["amPm"].Success) {
                    throw new FormatException("The string was not recognized as a valid time. The hour is in 12-hour notation but no AM/PM designator was found.");
                }

                var hour12 = Int32.Parse(m.Groups["hour12"].Value);
                var isPm = false;
                
                // Fix for some cultures on Windows 10 where both designators for some reason are empty strings.
                if (_dateTimeFormatProvider.AmPmDesignators[0] == _dateTimeFormatProvider.AmPmDesignators[1])
                    isPm = hour12 >= 12;
                else
                    isPm = m.Groups["amPm"].Value.Equals(_dateTimeFormatProvider.AmPmDesignators[1], StringComparison.InvariantCultureIgnoreCase);

                hour = ConvertToHour24(hour12, isPm);
            }

            if (m.Groups["minute"].Success) {
                minute = Int32.Parse(m.Groups["minute"].Value);
            }

            if (m.Groups["second"].Success) {
                second = Int32.Parse(m.Groups["second"].Value);
            }

            if (m.Groups["fraction"].Success) {
                millisecond = Convert.ToInt32(Decimal.Parse(String.Format(CultureInfo.InvariantCulture, "0.{0}", m.Groups["fraction"].Value), CultureInfo.InvariantCulture) * 1000);
            }

            if (m.Groups["timeZone"].Success) {
                var timeZoneString = m.Groups["timeZone"].Value;
                if (timeZoneString.ToUpperInvariant() == "Z") {
                    kind = DateTimeKind.Utc;
                }
                else {
                    offset = TimeSpan.Parse(timeZoneString.TrimStart('+'));
                    if (offset == CurrentTimeZone.BaseUtcOffset) {
                        kind = DateTimeKind.Local;
                    }
                    else if (offset == TimeSpan.Zero) {
                        kind = DateTimeKind.Utc;
                    }
                }
            }
            else if (m.Groups["offsetSign"].Success && m.Groups["offsetHours"].Success) {
                var offsetHours = Int32.Parse(m.Groups["offsetHours"].Value);
                var offsetMinutes = 0;
                if (m.Groups["offsetMinutes"].Success) {
                    offsetMinutes = Int32.Parse(m.Groups["offsetMinutes"].Value);
                }
                offset = new TimeSpan(offsetHours, offsetMinutes, 0);
                if (m.Groups["offsetSign"].Value == "-") {
                    offset = offset.Negate();
                }
                if (offset == CurrentTimeZone.BaseUtcOffset) {
                    kind = DateTimeKind.Local;
                }
                else if (offset == TimeSpan.Zero) {
                    kind = DateTimeKind.Utc;
                }
            }

            return new TimeParts(hour, minute, second, millisecond, kind, offset);
        }

        protected virtual void GetDateFormatValues(DateParts parts, Calendar calendar, out int twoDigitYear, out string monthName, out string monthNameShort, out string monthNameGenitive, out string monthNameShortGenitive, out string dayName, out string dayNameShort, out string eraName, out string eraNameShort) {
            var yearString = parts.Year.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
            twoDigitYear = Int32.Parse(yearString.Substring(yearString.Length - 2));
            monthName = parts.Month > 0 ? _dateTimeFormatProvider.MonthNames[parts.Month - 1] : null;
            monthNameShort = parts.Month > 0 ? _dateTimeFormatProvider.MonthNamesShort[parts.Month - 1] : null;
            monthNameGenitive = parts.Month > 0 ? _dateTimeFormatProvider.MonthNamesGenitive[parts.Month - 1] : null;
            monthNameShortGenitive = parts.Month > 0 ? _dateTimeFormatProvider.MonthNamesShortGenitive[parts.Month - 1] : null;
            dayName = parts.Day > 0 ? _dateTimeFormatProvider.DayNames[(int)calendar.GetDayOfWeek(parts.ToDateTime(calendar))] : null;
            dayNameShort = parts.Day > 0 ? _dateTimeFormatProvider.DayNamesShort[(int)calendar.GetDayOfWeek(parts.ToDateTime(calendar))] : null;
            eraName = parts.Day > 0 ? _dateTimeFormatProvider.GetEraName((int)calendar.GetEra(parts.ToDateTime(calendar))) : null;
            eraNameShort = parts.Day > 0 ? _dateTimeFormatProvider.GetShortEraName((int)calendar.GetEra(parts.ToDateTime(calendar))) : null;
        }

        protected virtual void GetTimeFormatValues(TimeParts parts, out bool isPm, out int hour12, out string amPm, out string amPmShort, out string timeZone, out string offsetSign, out int offsetHours, out int offsetMinutes, out string fraction1Zero, out string fraction2Zero, out string fraction3Zero, out string fraction1Digit, out string fraction2Digit, out string fraction3Digit) {
            hour12 = ConvertToHour12(parts.Hour, out isPm);
            amPm = _dateTimeFormatProvider.AmPmDesignators[isPm ? 1 : 0];
            amPmShort = String.IsNullOrEmpty(amPm) ? "" : amPm[0].ToString();
            offsetSign = parts.Offset.GetValueOrDefault() < TimeSpan.Zero ? "-" : "+";
            offsetHours = Math.Abs(parts.Offset.GetValueOrDefault().Hours);
            offsetMinutes = Math.Abs(parts.Offset.GetValueOrDefault().Minutes);
            timeZone = "";
            switch (parts.Kind) {
                case DateTimeKind.Utc:
                    timeZone = "Z";
                    break;
                case DateTimeKind.Local:
                    timeZone = String.Format("{0}{1:00}:{2:00}", offsetSign, offsetHours, offsetMinutes);
                    break;
            }
            fraction1Zero = (((decimal)parts.Millisecond) / 1000).ToString("0.0", CultureInfo.InvariantCulture).Substring(2);
            fraction2Zero = (((decimal)parts.Millisecond) / 1000).ToString("0.00", CultureInfo.InvariantCulture).Substring(2);
            fraction3Zero = (((decimal)parts.Millisecond) / 1000).ToString("0.000", CultureInfo.InvariantCulture).Substring(2);
            var fraction1 = (((decimal)parts.Millisecond) / 1000).ToString("0.#", CultureInfo.InvariantCulture);
            var fraction2 = (((decimal)parts.Millisecond) / 1000).ToString("0.##", CultureInfo.InvariantCulture);
            var fraction3 = (((decimal)parts.Millisecond) / 1000).ToString("0.###", CultureInfo.InvariantCulture);
            fraction1Digit = fraction1.Length >= 2 ? fraction1.Substring(2) : "";
            fraction2Digit = fraction2.Length >= 2 ? fraction2.Substring(2) : "";
            fraction3Digit = fraction3.Length >= 2 ? fraction3.Substring(2) : "";
        }

        protected virtual bool GetUseGenitiveMonthName(string format) {
            // Use genitive month name if the format (excluding literals) contains a numerical day component (d or dd).
            var formatWithoutLiterals = Regex.Replace(format, @"(?<!\\)'(.*?)(?<!\\)'|(?<!\\)""(.*?)(?<!\\)""", "", RegexOptions.Compiled);
            var numericalDayPattern = @"(\b|[^d])d{1,2}(\b|[^d])";
            return Regex.IsMatch(formatWithoutLiterals, numericalDayPattern, RegexOptions.Compiled);
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
                {"y", "(?<year>[0-9]{1})"},
                {"gg", String.Format("(?<eraName>{0})", String.Format("{0}|{1}", EscapeForRegex(_dateTimeFormatProvider.GetEraName(0)), EscapeForRegex(_dateTimeFormatProvider.GetEraName(1))))},
                {"g", String.Format("(?<eraName>{0})", String.Format("{0}|{1}", EscapeForRegex(_dateTimeFormatProvider.GetEraName(0)), EscapeForRegex(_dateTimeFormatProvider.GetEraName(1))))}
            };
        }

        protected virtual Dictionary<string, string> GetTimeParseReplacements() {
            var amDesignator = _dateTimeFormatProvider.AmPmDesignators[0];
            var pmDesignator = _dateTimeFormatProvider.AmPmDesignators[1];
            return new Dictionary<string, string>() {       
                {"HH", "(?<hour24>[0-9]{2})"},
                {"H", "(?<hour24>[0-9]{1,2})"},
                {"hh", "(?<hour12>[0-9]{2})"},
                {"h", "(?<hour12>[0-9]{1,2})"},
                {"mm", "(?<minute>[0-9]{2})"},
                {"m", "(?<minute>[0-9]{1,2})"},
                {"ss", "(?<second>[0-9]{2})"},      
                {"s", "(?<second>[0-9]{1,2})"},
                {"fffffff", "(?<fraction>[0-9]{7})"},
                {"ffffff", "(?<fraction>[0-9]{6})"},
                {"fffff", "(?<fraction>[0-9]{5})"},
                {"ffff", "(?<fraction>[0-9]{4})"},
                {"fff", "(?<fraction>[0-9]{3})"},
                {"ff", "(?<fraction>[0-9]{2})"},
                {"f", "(?<fraction>[0-9]{1})"},
                {"FFFFFFF", "(?<fraction>[0-9]{7})"},
                {"FFFFFF", "(?<fraction>[0-9]{6})"},
                {"FFFFF", "(?<fraction>[0-9]{5})"},
                {"FFFF", "(?<fraction>[0-9]{4})"},
                {"FFF", "(?<fraction>[0-9]{3})"},
                {"FF", "(?<fraction>[0-9]{2})"},
                {"F", "(?<fraction>[0-9]{1})"},
                {"tt", String.Format(@"\s*(?<amPm>{0}|{1})\s*", EscapeForRegex(amDesignator), EscapeForRegex(pmDesignator))},
                {"t", String.Format(@"\s*(?<amPm>{0}|{1})\s*", String.IsNullOrEmpty(amDesignator) ? "" : EscapeForRegex(amDesignator[0].ToString()), String.IsNullOrEmpty(pmDesignator) ? "" : EscapeForRegex(pmDesignator[0].ToString()))},
                {"K", @"(?<timeZone>Z|(\+|-)[0-9]{2}:[0-9]{2})*"},
                {"zzz", @"(?<offsetSign>\+|-)(?<offsetHours>[0-9]{2}):(?<offsetMinutes>[0-9]{2})"},
                {"zz", @"(?<offsetSign>\+|-)(?<offsetHours>[0-9]{2})"},
                {"z", @"(?<offsetSign>\+|-)(?<offsetHours>[0-9]{1,2})"}
            };
        }

        protected virtual Dictionary<string, string> GetDateFormatReplacements(bool useMonthNameGenitive) {
            return new Dictionary<string, string>() {       
                {"dddd", "{8}"},
                {"ddd", "{9}"},
                {"dd", "{7:00}"},
                {"d", "{7:##}"},
                {"MMMM", useMonthNameGenitive ? "{5}" : "{3}"},
                // The .NET formatting logic never uses the abbreviated genitive month name; doing the same for compatibility.
                //{"MMM", useMonthNameGenitive ? "{6}" : "{4}"},
                {"MMM", "{4}"},
                {"MM", "{2:00}"},
                {"M", "{2:##}"},
                {"yyyyy", "{0:00000}"},
                {"yyyy", "{0:0000}"},
                {"yyy", "{0:000}"},
                {"yy", "{1:00}"}, 
                {"y", "{1:0}"},
                {"gg", "{10}"},
                {"g", "{10}"}
            };
        }

        protected virtual Dictionary<string, string> GetTimeFormatReplacements() {
            return new Dictionary<string, string>() {       
                {"HH", "{12:00}"},
                {"H", "{12:#0}"},
                {"hh", "{13:00}"},
                {"h", "{13:#0}"},
                {"mm", "{14:00}"},
                {"m", "{14:#0}"},
                {"ss", "{15:00}"},      
                {"s", "{15:#0}"},
                {"fffffff", "{18}0000"},
                {"ffffff", "{18}000"},
                {"fffff", "{18}00"},
                {"ffff", "{18}0"},
                {"fff", "{18}"},
                {"ff", "{17}"},
                {"f", "{16}"},
                {"FFFFFFF", "{21}"},
                {"FFFFFF", "{21}"},
                {"FFFFF", "{21}"},
                {"FFFF", "{21}"},
                {"FFF", "{21}"},
                {"FF", "{20}"},
                {"F", "{19}"},
                {"tt", "{22}"},
                {"t", "{23}"},
                {"K", "{24}"},
                {"zzz", "{25}{26:00}:{27:00}"},
                {"zz", "{25}{26:00}"},
                {"z", "{25}{26:#0}"}
            };
        }

        protected virtual string ConvertToRegexPattern(string format, IDictionary<string, string> replacements) {
            string result = format;

            // Transform the / and : characters into culture-specific date and time separators.
            result = Regex.Replace(result, @"\/|:", m => m.Value == "/" ? _dateTimeFormatProvider.DateSeparator : _dateTimeFormatProvider.TimeSeparator, RegexOptions.Compiled);

            // Escape all characters that are intrinsic Regex syntax.
            result = EscapeForRegex(result);

            // Transform all literals to corresponding wildcard matches.
            result = Regex.Replace(result, @"(?<!\\)'(.*?)(?<!\\)'|(?<!\\)""(.*?)(?<!\\)""", m => String.Format("(?:.{{{0}}})", m.Value.Replace("\\", "").Length - 2), RegexOptions.Compiled);

            // Transform all DateTime format specifiers into corresponding Regex captures.
            result = result.ReplaceAll(replacements);

            // Make sure string is anchored to beginning and end.
            result = String.Format(@"^{0}$", result);

            return result;
        }

        protected virtual string ConvertToFormatString(string format, IDictionary<string, string> replacements) {
            string result = format;

            // * Transform all literals to corresponding text.
            // * Transform the / and : characters into culture-specific date and time separators.
            // * Transform all DateTime format specifiers into corresponding format string placeholders.
            // These three transformations must happen in one single pass, otherwise each will
            // re-transform the results of the previous one.
            var literalPattern = @"(?<!\\)'(.*?)(?<!\\)'|(?<!\\)""(.*?)(?<!\\)""";
            var separatorPattern = @"\/|:";
            var replacePattern = String.Format("{0}", String.Join("|", new[] { literalPattern, separatorPattern }.Concat(replacements.Keys)));
            result = Regex.Replace(result, replacePattern, m => {
                if (replacements.ContainsKey(m.Value)) { // Is it one of the format specifiers?
                    return replacements[m.Value];
                }
                if (m.Value == "/") { // Is it the date separator specifier?
                    return _dateTimeFormatProvider.DateSeparator;
                }
                if (m.Value == ":") { // Is it the time separator specifier?
                    return _dateTimeFormatProvider.TimeSeparator;
                }
                // Then it must be a literal.
                var literal = m.Value.Replace(@"\'", "'");
                return literal.Substring(1, literal.Length - 2);
            });

            return result;
        }

        protected virtual int ConvertToHour24(int hour12, bool isPm) {
            if (isPm) {
                return hour12 == 12 ? 12 : hour12 + 12;
            }
            return hour12 == 12 ? 0 : hour12;
        }

        protected virtual int ConvertToHour12(int hour24, out bool isPm) {
            if (hour24 >= 12) {
                isPm = true;
                return hour24 == 12 ? 12 : hour24 - 12;
            }
            isPm = false;
            return hour24 == 0 ? 12 : hour24;
        }

        protected virtual string EscapeForRegex(string input) {
            return Regex.Replace(input, @"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", m => String.Format(@"\{0}", m.Value), RegexOptions.Compiled);
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

        protected virtual TimeZoneInfo CurrentTimeZone {
            get {
                var workContext = _workContextAccessor.GetContext();
                return workContext.CurrentTimeZone;
            }
        }
    }
}
