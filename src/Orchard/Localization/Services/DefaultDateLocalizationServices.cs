using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Localization.Models;
using Orchard.Settings;

namespace Orchard.Localization.Services {

    public class DefaultDateLocalizationServices : IDateLocalizationServices {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IDateTimeFormatProvider _dateTimeFormatProvider;
        private readonly IDateFormatter _dateFormatter;
        private readonly ICalendarManager _calendarManager;

        public DefaultDateLocalizationServices(
            IWorkContextAccessor workContextAccessor,
            IDateTimeFormatProvider dateTimeFormatProvider,
            IDateFormatter dateFormatter,
            ICalendarManager calendarManager) {
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

        public virtual DateTimeParts ConvertToSiteCalendar(DateTime date) {
            var calendar = CurrentCalendar;
            return new DateTimeParts(
                calendar.GetYear(date),
                calendar.GetMonth(date),
                calendar.GetDayOfMonth(date),
                calendar.GetHour(date),
                calendar.GetMinute(date),
                calendar.GetSecond(date),
                Convert.ToInt32(calendar.GetMilliseconds(date)));
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
            return ConvertToLocalizedString(date, _dateTimeFormatProvider.LongTimeFormat, options);
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

            if (options.EnableTimeZoneConversion) {
                dateValue = ConvertToSiteTimeZone(dateValue);
            }

            var parts = DateTimeParts.FromDateTime(dateValue);
            if (options.EnableCalendarConversion && !(CurrentCalendar is GregorianCalendar)) {
                parts = ConvertToSiteCalendar(dateValue);
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

            var dateValue = parts.ToDateTime();
            if (hasDate && options.EnableCalendarConversion && !(CurrentCalendar is GregorianCalendar)) {
                dateValue = ConvertFromSiteCalendar(parts);
            }

            if (hasTime && options.EnableTimeZoneConversion) {
                dateValue = ConvertFromSiteTimeZone(dateValue);
            }

            return dateValue;
        }

        public DateTime? ConvertFromLocalizedString(string dateTimeString, DateLocalizationOptions options = null) {
            options = options ?? new DateLocalizationOptions();

            if (dateTimeString == null || dateTimeString == options.NullText) {
                return null;
            }

            var parts = _dateFormatter.ParseDateTime(dateTimeString);

            var dateValue = parts.ToDateTime();
            if (options.EnableCalendarConversion && !(CurrentCalendar is GregorianCalendar)) {
                dateValue = ConvertFromSiteCalendar(parts);
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