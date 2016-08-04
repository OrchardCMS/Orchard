using System;
using System.Globalization;
using Orchard.Localization.Models;
using Orchard.Services;

namespace Orchard.Localization.Services {

    public class DefaultDateLocalizationServices : IDateLocalizationServices {

        private readonly IClock _clock;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IDateTimeFormatProvider _dateTimeFormatProvider;
        private readonly IDateFormatter _dateFormatter;
        private readonly ICalendarManager _calendarManager;

        public DefaultDateLocalizationServices(
            IClock clock,
            IWorkContextAccessor workContextAccessor,
            IDateTimeFormatProvider dateTimeFormatProvider,
            IDateFormatter dateFormatter,
            ICalendarManager calendarManager) {
            _clock = clock;
            _workContextAccessor = workContextAccessor;
            _dateTimeFormatProvider = dateTimeFormatProvider;
            _dateFormatter = dateFormatter;
            _calendarManager = calendarManager;
        }

        public virtual DateTime ConvertToSiteTimeZone(DateTime dateUtc) {
            // Some trickery is necessary for correct handling of DateTimeKind values
            // of both input and output dates, because the TimeZoneInfo class' internal
            // handling of this is tightly coupled to the configured time zone of
            // the local computer, which can differ from the configured time zone of
            // the Orchard site.
            if (dateUtc.Kind == DateTimeKind.Local) {
                return dateUtc;
            }
            if (CurrentTimeZone == TimeZoneInfo.Utc) {
                if (dateUtc.Kind == DateTimeKind.Unspecified) {
                    return DateTime.SpecifyKind(dateUtc, DateTimeKind.Utc);
                }
                return dateUtc;
            }
            var dateLocal = TimeZoneInfo.ConvertTimeFromUtc(dateUtc, CurrentTimeZone);
            return DateTime.SpecifyKind(dateLocal, DateTimeKind.Local);
        }

        public virtual DateTime ConvertFromSiteTimeZone(DateTime dateLocal) {
            // Some trickery is necessary for correct handling of DateTimeKind values
            // of both input and output dates, because the TimeZoneInfo class' internal
            // handling of this is tightly coupled to the configured time zone of
            // the local computer, which can differ from the configured time zone of
            // the Orchard site.
            if (dateLocal.Kind == DateTimeKind.Utc) {
                return dateLocal;
            }
            var dateUnspecified = new DateTime(dateLocal.Ticks, DateTimeKind.Unspecified);
            var dateUtc = TimeZoneInfo.ConvertTimeToUtc(dateUnspecified, CurrentTimeZone);
            return DateTime.SpecifyKind(dateUtc, DateTimeKind.Utc);
        }

        public virtual DateTimeParts ConvertToSiteCalendar(DateTime date, TimeSpan offset) {
            var calendar = CurrentCalendar;
            return new DateTimeParts(
                calendar.GetYear(date),
                calendar.GetMonth(date),
                calendar.GetDayOfMonth(date),
                calendar.GetHour(date),
                calendar.GetMinute(date),
                calendar.GetSecond(date),
                Convert.ToInt32(calendar.GetMilliseconds(date)),
                date.Kind,
                offset);
        }

        public virtual DateTime ConvertFromSiteCalendar(DateTimeParts parts) {
            return new DateTime(
                parts.Date.Year,
                parts.Date.Month,
                parts.Date.Day,
                parts.Time.Hour,
                parts.Time.Minute,
                parts.Time.Second,
                parts.Time.Millisecond,
                CurrentCalendar,
                parts.Time.Kind);
        }

        public string ConvertToLocalizedDateString(DateTime date, DateLocalizationOptions options = null) {
            return ConvertToLocalizedDateString(ToNullable(date), options);
        }

        public string ConvertToLocalizedDateString(DateTime? date, DateLocalizationOptions options = null) {
            return ConvertToLocalizedString(date, _dateTimeFormatProvider.ShortDateFormat, options);
        }

        public string ConvertToLocalizedTimeString(DateTime date, DateLocalizationOptions options = null) {
            return ConvertToLocalizedTimeString(ToNullable(date), options);
        }

        public string ConvertToLocalizedTimeString(DateTime? date, DateLocalizationOptions options = null) {
            options = options ?? new DateLocalizationOptions();

            if (!date.HasValue) {
                return options.NullText;
            }

            var dateValue = date.Value;
            var offset = TimeSpan.Zero;

            if (options.EnableTimeZoneConversion) {
                if (options.IgnoreDate) {
                    // The caller has asked us to ignore the date part. This usually because the source
                    // is a time-only field. In such cases (with an undefined date) it does not make sense
                    // to consider DST variations throughout the year, so we will use an arbitrary (but fixed)
                    // non-DST date for the conversion to ensure DST is never applied during conversion. The
                    // date part is usually DateTime.MinValue which we should not use because time zone
                    // conversion cannot wrap DateTime.MinValue around to the previous day, resulting in
                    // an undefined result. Instead we convert the date to a hard-coded date of 2000-01-01
                    // before the conversion, and back to the original date after.
                    var tempDate = new DateTime(2000, 1, 1, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);
                    tempDate = ConvertToSiteTimeZone(tempDate);
                    dateValue = new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, tempDate.Hour, tempDate.Minute, tempDate.Second, tempDate.Millisecond, tempDate.Kind);
                }
                else {
                    dateValue = ConvertToSiteTimeZone(dateValue);
                }

                offset = CurrentTimeZone.GetUtcOffset(date.Value);
            }

            var parts = DateTimeParts.FromDateTime(dateValue, offset);

            // INFO: No calendar conversion in this method - we expect the date component to be DateTime.MinValue and irrelevant anyway.

            return _dateFormatter.FormatDateTime(parts, _dateTimeFormatProvider.LongTimeFormat);
        }

        public string ConvertToLocalizedString(DateTime date, DateLocalizationOptions options = null) {
            return ConvertToLocalizedString(ToNullable(date), options);
        }

        public string ConvertToLocalizedString(DateTime? date, DateLocalizationOptions options = null) {
            return ConvertToLocalizedString(date, _dateTimeFormatProvider.ShortDateTimeFormat, options);
        }

        public string ConvertToLocalizedString(DateTime date, string format, DateLocalizationOptions options = null) {
            return ConvertToLocalizedString(ToNullable(date), format, options);
        }

        public string ConvertToLocalizedString(DateTime? date, string format, DateLocalizationOptions options = null) {
            options = options ?? new DateLocalizationOptions();

            if (!date.HasValue) {
                return options.NullText;
            }

            var dateValue = date.Value;
            var offset = TimeSpan.Zero;

            if (options.EnableTimeZoneConversion) {
                dateValue = ConvertToSiteTimeZone(dateValue);
                offset = CurrentTimeZone.GetUtcOffset(date.Value);
            }

            var parts = DateTimeParts.FromDateTime(dateValue, offset);
            if (options.EnableCalendarConversion && !(CurrentCalendar is GregorianCalendar)) {
                parts = ConvertToSiteCalendar(dateValue, offset);
            }

            return _dateFormatter.FormatDateTime(parts, format);
        }

        public DateTime? ConvertFromLocalizedDateString(string dateString, DateLocalizationOptions options = null) {
            return ConvertFromLocalizedString(dateString, null, options);
        }

        public DateTime? ConvertFromLocalizedTimeString(string timeString, DateLocalizationOptions options = null) {
            return ConvertFromLocalizedString(null, timeString, options);
        }

        public DateTime? ConvertFromLocalizedString(string dateString, string timeString, DateLocalizationOptions options = null) {
            options = options ?? new DateLocalizationOptions();

            var hasDate = dateString != null && dateString != options.NullText;
            var hasTime = timeString != null && timeString != options.NullText;
            if (!hasDate && !hasTime) {
                return null;
            }

            var parts = new DateTimeParts(
                hasDate ? _dateFormatter.ParseDate(dateString) : DateParts.MinValue,
                hasTime ? _dateFormatter.ParseTime(timeString) : TimeParts.MinValue
            );

            DateTime dateValue;
            if (hasDate && options.EnableCalendarConversion && !(CurrentCalendar is GregorianCalendar)) {
                dateValue = ConvertFromSiteCalendar(parts);
            }
            else {
                dateValue = parts.ToDateTime(new GregorianCalendar());
            }

            if (hasTime && options.EnableTimeZoneConversion) {
                // If there is no date component (technically the date component is that of DateTime.MinValue)
                // then we must employ some trickery. With an undefined date it does not make sense
                // to consider DST variations throughout the year, so we will use an arbitrary (but fixed)
                // non-DST date for the conversion to ensure DST is never applied during conversion. The
                // date part is usually DateTime.MinValue which we should not use because time zone
                // conversion cannot wrap DateTime.MinValue around to the previous day, resulting in
                // an undefined result. Instead we convert the date to a hard-coded date of 2000-01-01
                // before the conversion, and back to the original date after.
                if (!hasDate) {
                    dateValue = new DateTime(2000, 1, 1, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);
                }
                dateValue = ConvertFromSiteTimeZone(dateValue);
                if (!hasDate) {
                    dateValue = new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);
                }
            }

            if (options.EnableTimeZoneConversion)
                dateValue = DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);

            return dateValue;
        }

        public DateTime? ConvertFromLocalizedString(string dateTimeString, DateLocalizationOptions options = null) {
            options = options ?? new DateLocalizationOptions();

            if (dateTimeString == null || dateTimeString == options.NullText) {
                return null;
            }

            var parts = _dateFormatter.ParseDateTime(dateTimeString);

            DateTime dateValue;
            if (options.EnableCalendarConversion && !(CurrentCalendar is GregorianCalendar)) {
                dateValue = ConvertFromSiteCalendar(parts);
            }
            else {
                dateValue = parts.ToDateTime(new GregorianCalendar());
            }

            if (options.EnableTimeZoneConversion) {
                dateValue = ConvertFromSiteTimeZone(dateValue);
            }

            return dateValue;
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

        protected virtual DateTime? ToNullable(DateTime date) {
            return date == DateTime.MinValue ? new DateTime?() : new DateTime?(date);
        }
    }
}