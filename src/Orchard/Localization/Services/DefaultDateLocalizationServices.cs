using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Services;
using Orchard.Settings;

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

        public virtual DateTime ConvertToSiteTimeZone(DateTime date) {
            var workContext = _workContextAccessor.GetContext();
            return TimeZoneInfo.ConvertTimeFromUtc(date, workContext.CurrentTimeZone);
        }

        public virtual DateTime ConvertFromSiteTimeZone(DateTime date) {
            var workContext = _workContextAccessor.GetContext();
            return TimeZoneInfo.ConvertTimeToUtc(date, workContext.CurrentTimeZone);
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
                DateTimeKind.Utc,
                offset);
        }

        public virtual DateTime ConvertFromSiteCalendar(DateTimeParts parts) {
            return CurrentCalendar.ToDateTime(parts.Date.Year, parts.Date.Month, parts.Date.Day, parts.Time.Hour, parts.Time.Minute, parts.Time.Second, parts.Time.Millisecond);
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
                // Since no date component is expected (technically the date component is that of DateTime.MinValue) then
                // we must employ some trickery, for two reasons:
                // * DST can be active or not dependeng on the time of the year. We want the conversion to always act as if the time represents today, but we don't want that date stored.
                // * Time zone conversion cannot wrap DateTime.MinValue around to the previous day, resulting in undefined result.
                // Therefore we convert the date to today's date before the conversion, and back to DateTime.MinValue after.
                var now = _clock.UtcNow;

                dateValue = new DateTime(now.Year, now.Month, now.Day, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);
                dateValue = ConvertToSiteTimeZone(dateValue);
                dateValue = new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);

                var workContext = _workContextAccessor.GetContext();
                offset = workContext.CurrentTimeZone.BaseUtcOffset;
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
                var workContext = _workContextAccessor.GetContext();
                offset = workContext.CurrentTimeZone.BaseUtcOffset;
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
                // If there is no date component (technically the date component is that of DateTime.MinValue) then
                // we must employ some trickery, for two reasons:
                // * DST can be active or not dependeng on the time of the year. We want the conversion to always act as if the time represents today, but we don't want that date stored.
                // * Time zone conversion cannot wrap DateTime.MinValue around to the previous day, resulting in undefined result.
                // Therefore we convert the date to today's date before the conversion, and back to DateTime.MinValue after.
                if (!hasDate) {
                    var now = _clock.UtcNow;
                    dateValue = new DateTime(now.Year, now.Month, now.Day, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);
                }
                dateValue = ConvertFromSiteTimeZone(dateValue);
                if (!hasDate) {
                    dateValue = new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, dateValue.Hour, dateValue.Minute, dateValue.Second, dateValue.Millisecond, dateValue.Kind);
                }
            }

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

        protected virtual DateTime? ToNullable(DateTime date) {
            return date == DateTime.MinValue ? new DateTime?() : new DateTime?(date);
        }
    }
}