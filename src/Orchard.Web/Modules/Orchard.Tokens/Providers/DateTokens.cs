using System;
using System.Globalization;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Mvc.Html;
using Orchard.Services;

namespace Orchard.Tokens.Providers {
    public class DateTokens : ITokenProvider {
        private readonly IClock _clock;
        private readonly IDateTimeFormatProvider _dateTimeLocalization;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;
        private readonly IDateServices _dateServices;

        public DateTokens(
            IClock clock, 
            IDateTimeFormatProvider dateTimeLocalization, 
            IWorkContextAccessor workContextAccessor,
            IDateServices dateServices) {
            _clock = clock;
            _dateTimeLocalization = dateTimeLocalization;
            _workContextAccessor = workContextAccessor;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));
            _dateServices = dateServices;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Date", T("Date/time"), T("Current date/time tokens"))
                .Token("Since", T("Since"), T("Relative to the current date/time."), "Date")
                .Token("Local", T("Local"), T("Based on the configured time zone and calendar."), "Date")
                .Token("ShortDate", T("Short Date"), T("Short date format."))
                .Token("ShortTime", T("Short Time"), T("Short time format."))
                .Token("Long", T("Long Date and Time"), T("Long date and time format."))
                .Token("Format:*", T("Format:<date format>"), T("Optional format specifier (e.g. yyyy/MM/dd). See format strings at <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/az4se3k1.aspx\">Standard Formats</a> and <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx\">Custom Formats</a>"), "DateTime");
        }

        public void Evaluate(EvaluateContext context) {
            context.For("Date", () => _clock.UtcNow)
                // {Date.Since}
                .Token("Since", DateTimeRelative)
                .Chain("Since", "Date", DateTimeRelative)
                // {Date.Local}
                .Token("Local", d => _dateServices.ConvertToLocal(d))
                .Chain("Local", "Date", d => _dateServices.ConvertToLocal(d))
                // {Date.ShortDate}
                .Token("ShortDate", d => d.ToString(_dateTimeLocalization.ShortDateFormat, _cultureInfo.Value))
                // {Date.ShortTime}
                .Token("ShortTime", d => d.ToString(_dateTimeLocalization.ShortTimeFormat, _cultureInfo.Value))
                // {Date.Long}
                .Token("Long", d => d.ToString(_dateTimeLocalization.LongDateTimeFormat, _cultureInfo.Value))
                // {Date}
                .Token(
                    token => token == String.Empty ? String.Empty : null,
                    (token, d) => d.ToString(_dateTimeLocalization.ShortDateFormat + " " + _dateTimeLocalization.ShortTimeFormat, _cultureInfo.Value))
                // {Date.Format:<formatstring>}
                .Token(
                    token => token.StartsWith("Format:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Format:".Length) : null,
                    (token, d) => d.ToString(token, _cultureInfo.Value));
        }

        private string DateTimeRelative(DateTime dateTimeUtc) {
            var time = _clock.UtcNow - dateTimeUtc.ToUniversalTime();

            if (time.TotalDays > 7)
                return dateTimeUtc.ToString(T("'on' MMM d yyyy 'at' h:mm tt").ToString(), _cultureInfo.Value);
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