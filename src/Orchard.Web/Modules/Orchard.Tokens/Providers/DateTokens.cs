using System;
using System.Globalization;
using Orchard.Localization;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Mvc.Html;
using Orchard.Services;

namespace Orchard.Tokens.Providers {
    public class DateTokens : ITokenProvider {
        private readonly IClock _clock;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IDateTimeFormatProvider _dateTimeFormats;
        private readonly IDateFormatter _dateFormatter;
        private readonly IDateLocalizationServices _dateLocalizationServices;

        //private readonly Lazy<CultureInfo> _cultureInfo;

        public DateTokens(
            IClock clock, 
            IWorkContextAccessor workContextAccessor,
            IDateTimeFormatProvider dateTimeFormats, 
            IDateFormatter dateFormatter,
            IDateLocalizationServices dateLocalizationServices) {
            _clock = clock;
            _workContextAccessor = workContextAccessor;
            _dateTimeFormats = dateTimeFormats;
            _dateFormatter = dateFormatter;
            _dateLocalizationServices = dateLocalizationServices;

            //_cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));
            
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Date", T("Date/Time"), T("Current date/time tokens"))
                .Token("Since", T("Since"), T("Relative to the current date/time."), "Date")
                .Token("Local", T("Local"), T("Based on the configured time zone and calendar."), "Date")
                .Token("Short", T("Short Date and Time"), T("Short date and time format."))
                .Token("ShortDate", T("Short Date"), T("Short date format."))
                .Token("ShortTime", T("Short Time"), T("Short time format."))
                .Token("Long", T("Long Date and Time"), T("Long date and time format."))
                .Token("LongDate", T("Long Date"), T("Long date format."))
                .Token("LongTime", T("Long Time"), T("Long time format."))
                .Token("Format:*", T("Format:<formatString>"), T("Optional custom date/time format string (e.g. yyyy/MM/dd). For reference see <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx\">Custom Date and Time Format Strings</a>"), "DateTime");
        }

        public void Evaluate(EvaluateContext context) {
            context.For("Date", () => _clock.UtcNow)
                // {Date.Since}
                .Token("Since", DateTimeRelative)
                // {Date.Local}
                .Token("Local", d => _dateLocalizationServices.ConvertToLocalizedString(d))
                .Chain("Local", "Date", d => _dateLocalizationServices.ConvertToSiteTimeZone(d))
                // {Date.Short}
                .Token("Short", d => _dateLocalizationServices.ConvertToLocalizedString(d, _dateTimeFormats.ShortDateTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date.ShortDate}
                .Token("ShortDate", d => _dateLocalizationServices.ConvertToLocalizedString(d, _dateTimeFormats.ShortDateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date.ShortTime}
                .Token("ShortTime", d => _dateLocalizationServices.ConvertToLocalizedString(d, _dateTimeFormats.ShortTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date.Long}
                .Token("Long", d => _dateLocalizationServices.ConvertToLocalizedString(d, _dateTimeFormats.LongDateTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date.LongDate}
                .Token("LongDate", d => _dateLocalizationServices.ConvertToLocalizedString(d, _dateTimeFormats.LongDateFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date.LongTime}
                .Token("LongTime", d => _dateLocalizationServices.ConvertToLocalizedString(d, _dateTimeFormats.LongTimeFormat, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date}
                .Token(
                    token => token == String.Empty ? String.Empty : null,
                    (token, d) => _dateLocalizationServices.ConvertToLocalizedString(d, new DateLocalizationOptions() { EnableTimeZoneConversion = false }))
                // {Date.Format:<formatString>}
                .Token(
                    token => token.StartsWith("Format:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Format:".Length) : null,
                    (token, d) => _dateLocalizationServices.ConvertToLocalizedString(d, token, new DateLocalizationOptions() { EnableTimeZoneConversion = false }));
        }

        private string DateTimeRelative(DateTime dateTimeUtc) {
            var time = _clock.UtcNow - dateTimeUtc.ToUniversalTime();

            if (time.TotalDays > 7)
                return _dateLocalizationServices.ConvertToLocalizedString(dateTimeUtc, T("'on' MMM d yyyy 'at' h:mm tt").Text);
            if (time.TotalHours > 24)
                return T.Plural("1 day ago", "{0} days ago", time.Days).ToString();
            if (time.TotalMinutes > 60)
                return T.Plural("1 hour ago", "{0} hours ago", time.Hours).ToString();
            if (time.TotalSeconds > 60)
                return T.Plural("1 minute ago", "{0} minutes ago", time.Minutes).ToString();
            if (time.TotalSeconds > 10)
                return T.Plural("1 second ago", "{0} seconds ago", time.Seconds).ToString();

            return T("a moment ago").ToString();
        }
    }
}