using System;
using System.Web;
using System.Web.Mvc;
using Orchard.Core.Shapes.Localization;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Services;
using System.Globalization;

namespace Orchard.Core.Shapes {
    public class DateTimeShapes : IDependency {
        private readonly IClock _clock;
        private readonly IDateTimeLocalization _dateTimeLocalization;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public DateTimeShapes(
            IClock clock,
            IDateTimeLocalization dateTimeLocalization,
            IWorkContextAccessor workContextAccessor
            ) {
            _clock = clock;
            _dateTimeLocalization = dateTimeLocalization;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;

            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture));
        }

        public Localizer T { get; set; }

        [Shape]
        public IHtmlString DateTimeRelative(dynamic Display, DateTime dateTimeUtc) {
            var time = _clock.UtcNow - dateTimeUtc;

            if (time.TotalDays > 7)
                return Display.DateTime(DateTimeUtc: dateTimeUtc, CustomFormat: T("'on' MMM d yyyy 'at' h:mm tt"));
            if (time.TotalHours > 24)
                return T.Plural("1 day ago", "{0} days ago", time.Days);
            if (time.TotalMinutes > 60)
                return T.Plural("1 hour ago", "{0} hours ago", time.Hours);
            if (time.TotalSeconds > 60)
                return T.Plural("1 minute ago", "{0} minutes ago", time.Minutes);
            if (time.TotalSeconds > 10)
                return T.Plural("1 second ago", "{0} seconds ago", time.Seconds); //aware that the singular won't be used

            return T("a moment ago");
        }

        [Shape]
        public IHtmlString DateTime(DateTime DateTimeUtc, LocalizedString CustomFormat) {
            //using a LocalizedString forces the caller to use a localizable format

            if (CustomFormat == null || String.IsNullOrWhiteSpace(CustomFormat.Text)) {
                return DateTime(DateTimeUtc, _dateTimeLocalization.LongDateTimeFormat);
            }

            return new MvcHtmlString(ConvertToDisplayTime(DateTimeUtc).ToString(CustomFormat.Text, _cultureInfo.Value));
        }

        /// <summary>
        /// Converts a Coordinated Universal Time (UTC) to the time in the current time zone.
        /// </summary>
        /// <param name="dateTimeUtc">The Coordinated Universal Time (UTC).</param>
        /// <returns>The date and time in the selected time zone. Its System.DateTime.Kind property is System.DateTimeKind.Utc if the current zone is System.TimeZoneInfo.Utc; otherwise, its System.DateTime.Kind property is System.DateTimeKind.Unspecified.</returns>
        private DateTime ConvertToDisplayTime(DateTime dateTimeUtc) {

            // get the time zone for the current request
            var timeZone = _workContextAccessor.GetContext().CurrentTimeZone;

            return TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, timeZone);
        }

    }
}
