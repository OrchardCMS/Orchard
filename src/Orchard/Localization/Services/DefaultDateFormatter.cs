using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Orchard.Framework.Localization.Models;

namespace Orchard.Framework.Localization.Services {
    public class DefaultDateFormatter : IDateFormatter {

        DateTimeParts IDateFormatter.ParseDateTime(string dateTimeString, CultureInfo culture) {
            DateParts date = ((IDateFormatter)this).ParseDate(dateTimeString, culture);
            TimeParts time = ((IDateFormatter)this).ParseTime(dateTimeString, culture);
            return new DateTimeParts(date, time);
        }

        DateParts IDateFormatter.ParseDate(string dateString, CultureInfo culture) {
            var dateFormatString = Regex.Replace(culture.DateTimeFormat.ShortDatePattern, @"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", m => String.Format(@"\{0}", m.Value));
            dateFormatString = Regex.Replace(dateFormatString, @"(?<!\\)'(.*?)((?<!\\)')", m => String.Format("(.{{{0}}})", m.Value.Replace("\\", "").Length - 2));

            var dateFormat = ReplaceAll(dateFormatString, GetDateParseReplacements(culture));

            if (!Regex.IsMatch(dateString, dateFormat, RegexOptions.IgnoreCase))
                throw new FormatException("Invalid date format.");

            Match dateMatch = Regex.Match(dateString, dateFormat, RegexOptions.IgnoreCase);

            int year = 0,
                month = 0,
                day = 0;

            if (dateMatch.Groups["year"].Success) {
                int.TryParse(dateMatch.Groups["year"].Value, out year);
                year = culture.DateTimeFormat.Calendar.ToFourDigitYear(year);
            }
            if (dateMatch.Groups["month"].Success) {
                int.TryParse(dateMatch.Groups["month"].Value, out month);
            }
            if (dateMatch.Groups["day"].Success) {
                int.TryParse(dateMatch.Groups["day"].Value, out day);
            }

            return new DateParts(year, month, day);
        }

        TimeParts IDateFormatter.ParseTime(string timeString, CultureInfo culture) {
            var timeFormatString = Regex.Replace(culture.DateTimeFormat.LongTimePattern, @"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\", m => String.Format(@"\{0}", m.Value));
            timeFormatString = Regex.Replace(timeFormatString, @"(?<!\\)'(.*?)((?<!\\)')", m => String.Format("(.{{{0}}})", m.Value.Replace("\\", "").Length - 2));

            var timeFormat = ReplaceAll(timeFormatString, GetTimeParseReplacements(culture));

            if (!Regex.IsMatch(timeString, timeFormat, RegexOptions.IgnoreCase))
                throw new FormatException("Invalid time format.");

            Match timeMatch = Regex.Match(timeString, timeFormat, RegexOptions.IgnoreCase);

            int hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0;

            if (timeMatch.Groups["hour"].Success) {
                int.TryParse(timeMatch.Groups["hour"].Value, out hour);
            }
            if (timeMatch.Groups["minute"].Success) {
                int.TryParse(timeMatch.Groups["minute"].Value, out minute);
            }
            if (timeMatch.Groups["second"].Success) {
                int.TryParse(timeMatch.Groups["second"].Value, out second);
            }
            if (timeMatch.Groups["millisecond"].Success) {
                int.TryParse(timeMatch.Groups["millisecond"].Value, out millisecond);
            }

            return new TimeParts(hour, minute, second, millisecond);
        }

        string IDateFormatter.FormatDateTime(DateTimeParts parts, CultureInfo culture) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        string IDateFormatter.FormatDate(DateParts parts, CultureInfo culture) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        string IDateFormatter.FormatTime(TimeParts parts, CultureInfo culture) {
            // TODO: Mahsa should implement!
            throw new NotImplementedException();
        }

        private Dictionary<string, string> GetDateParseReplacements(CultureInfo culture) {
            return new Dictionary<string, string>() {       
                {"dddd", "(?<day>[0-9]{4})"},
                {"ddd", "(?<day>[0-9]{3})"},
                {"dd", "(?<day>[0-9]{2})"},
                {"d", "(?<day>[0-9]{1,2})"},
                {"MMMM", String.Format("(?<month>{0})", String.Join("|", culture.DateTimeFormat.MonthNames))},
                {"MMM", String.Format("(?<month>{0})", String.Join("|", culture.DateTimeFormat.AbbreviatedMonthNames))},
                {"MM", "(?<month>[0-9]{2})"},
                {"M", "(?<month>[0-9]{1,2})"},
                {"yyyyy", "(?<year>[0-9]{5})"},
                {"yyyy", "(?<year>[0-9]{4})"},
                {"yyy", "(?<year>[0-9]{3})"},
                {"yy", "(?<year>[0-9]{2})"}, 
                {"y", "(?<year>[0-9]{1})"}
            };
        }

        private Dictionary<string, string> GetTimeParseReplacements(CultureInfo culture) {
            return new Dictionary<string, string>() {       
                {"HH", "(?<hour>[0-9]{2})"},
                {"H", "(?<hour>[0-9]{1,2})"},
                {"hh", "(?<hour>[0-9]{2})"},
                {"h", "(?<hour>[0-9]{1,2})"},
                {"MM", "(?<minute>[0-9]{2})"},
                {"M", "(?<minute>[0-9]{1,2})"},
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
                {"tt", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", culture.DateTimeFormat.AMDesignator, culture.DateTimeFormat.PMDesignator)},
                {"t", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", culture.DateTimeFormat.AMDesignator, culture.DateTimeFormat.PMDesignator)},
                {" tt", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", culture.DateTimeFormat.AMDesignator, culture.DateTimeFormat.PMDesignator)},
                {" t", String.Format("\\s*(?<AMPM>{0}|{1})\\s*", culture.DateTimeFormat.AMDesignator, culture.DateTimeFormat.PMDesignator)}
            };
        }

        private string ReplaceAll(string original, IDictionary<string, string> replacements) {
            var pattern = String.Format("{0}", String.Join("|", replacements.Keys.ToArray()));
            return Regex.Replace(original, pattern, (match) => replacements[match.Value]);
        }
    }
}
