using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Framework.Localization.Models;
using Orchard.Settings;

namespace Orchard.Localization.Services {

    public class DefaultDateLocalizationServices : IDateLocalizationServices {

        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IDateTimeFormatProvider _dateTimeLocalization;
        private readonly ICalendarManager _calendarManager;

        public DefaultDateLocalizationServices(
            IWorkContextAccessor workContextAccessor,
            IDateTimeFormatProvider dateTimeLocalization,
            ICalendarManager calendarManager) {
            _workContextAccessor = workContextAccessor;
            _dateTimeLocalization = dateTimeLocalization;
            _calendarManager = calendarManager;
        }

        public virtual DateTime? ConvertToSiteTimeZone(DateTime date) {
            return ConvertToSiteTimeZone(ToNullable(date));
        }

        public virtual DateTime? ConvertToSiteTimeZone(DateTime? date) {
            if (!date.HasValue) {
                return null;
            }
            var workContext = _workContextAccessor.GetContext();
            return TimeZoneInfo.ConvertTimeFromUtc(date.Value, workContext.CurrentTimeZone);
        }

        public virtual DateTime? ConvertFromSiteTimeZone(DateTime date) {
            return ConvertFromSiteTimeZone(ToNullable(date));
        }

        public virtual DateTime? ConvertFromSiteTimeZone(DateTime? date) {
            if (!date.HasValue) {
                return null;
            }
            var workContext = _workContextAccessor.GetContext();
            return TimeZoneInfo.ConvertTimeToUtc(date.Value, workContext.CurrentTimeZone);
        }

        public virtual DateTimeParts? ConvertToSiteCalendar(DateTime? date) {
            if (!date.HasValue){
                return null;
            }
            var calendar = CurrentCalendar;
            return new DateTimeParts(
                calendar.GetYear(date.Value),
                calendar.GetMonth(date.Value),
                calendar.GetDayOfMonth(date.Value),
                calendar.GetHour(date.Value),
                calendar.GetMinute(date.Value),
                calendar.GetSecond(date.Value),
                Convert.ToInt32(calendar.GetMilliseconds(date.Value)));
        }

        public virtual DateTime? ConvertFromSiteCalendar(DateTimeParts? parts) {
            if (!parts.HasValue) {
                return null;
            }
            var calendar = CurrentCalendar;
            return new DateTime(parts.Value.Date.Year, parts.Value.Date.Month, parts.Value.Date.Day, parts.Value.Time.Hour, parts.Value.Time.Minute, parts.Value.Time.Second, parts.Value.Time.Millisecond, calendar);
        }



        public virtual string ConvertToLocalString(DateTime date, string nullText = null) {
            return ConvertToLocalString(ToNullable(date), _dateTimeLocalization.LongDateTimeFormat, nullText);
        }

        public virtual string ConvertToLocalString(DateTime date, string format, string nullText = null) {
            return ConvertToLocalString(ToNullable(date), format, nullText);
        }

        public virtual string ConvertToLocalString(DateTime? date, string format, string nullText = null) {
            var localDate = ConvertToSiteTimeZone(date);
            if (!localDate.HasValue) {
                return nullText;
            }

            // If the configured current calendar is different from the default calendar
            // of the configured current culture we must override the conversion process. 
            // We do this by using a custom CultureInfo modified to use GregorianCalendar
            // (means no calendar conversion will be performed as part of the string
            // formatting) and instead perform the calendar conversion ourselves.

            var cultureInfo = CurrentCulture;
            var usingCultureCalendar = CurrentCulture.DateTimeFormat.Calendar.GetType().IsInstanceOfType(CurrentCalendar);
            if (!usingCultureCalendar) {
                cultureInfo = (CultureInfo)CurrentCulture.Clone();
                cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
                var calendar = CurrentCalendar;
                localDate = new DateTime(calendar.GetYear(localDate.Value), calendar.GetMonth(localDate.Value), calendar.GetDayOfMonth(localDate.Value), calendar.GetHour(localDate.Value), calendar.GetMinute(localDate.Value), calendar.GetSecond(localDate.Value));
            }

            return localDate.Value.ToString(format, cultureInfo);
        }

        public virtual string ConvertToLocalDateString(DateTime date, string nullText = null) {
            return ConvertToLocalDateString(ToNullable(date), nullText);
        }

        public virtual string ConvertToLocalDateString(DateTime? date, string nullText = null) {
            return ConvertToLocalString(date, _dateTimeLocalization.ShortDateFormat, nullText);
        }

        public virtual string ConvertToLocalTimeString(DateTime date, string nullText = null) {
            return ConvertToLocalTimeString(ToNullable(date), nullText);
        }

        public virtual string ConvertToLocalTimeString(DateTime? date, string nullText = null) {
            return ConvertToLocalString(date, _dateTimeLocalization.ShortTimeFormat, nullText);
        }

        public virtual DateTime? ConvertFromLocalString(string dateString) {
            if (String.IsNullOrWhiteSpace(dateString)) {
                return null;
            }

            // If the configured current calendar is different from the default calendar
            // of the configured current culture we must override the conversion process. 
            // We do this by using a custom CultureInfo modified to use GregorianCalendar
            // (means no calendar conversion will be performed as part of the string
            // parsing) and instead perform the calendar conversion ourselves.

            var cultureInfo = CurrentCulture;
            var usingCultureCalendar = CurrentCulture.DateTimeFormat.Calendar.GetType().IsInstanceOfType(CurrentCalendar);
            if (!usingCultureCalendar) {
                cultureInfo = (CultureInfo)CurrentCulture.Clone();
                cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
            }

            var localDate = DateTime.Parse(dateString, CurrentCulture);

            if (!usingCultureCalendar) {
                var calendar = CurrentCalendar;
                localDate = calendar.ToDateTime(localDate.Year, localDate.Month, localDate.Day, localDate.Hour, localDate.Minute, localDate.Second, localDate.Millisecond);
            }

            return ConvertFromSiteTimeZone(localDate);
        }

        public virtual DateTime? ConvertFromLocalString(string dateString, string timeString) {
            if (String.IsNullOrWhiteSpace(dateString) && String.IsNullOrWhiteSpace(timeString)) {
                return null;
            }

            // If the configured current calendar is different from the default calendar
            // of the configured current culture we must override the conversion process. 
            // We do this by using a custom CultureInfo modified to use GregorianCalendar
            // (means no calendar conversion will be performed as part of the string
            // parsing) and instead perform the calendar conversion ourselves.

            var cultureInfo = CurrentCulture;
            var usingCultureCalendar = CurrentCulture.DateTimeFormat.Calendar.GetType().IsInstanceOfType(CurrentCalendar);
            if (!usingCultureCalendar) {
                cultureInfo = (CultureInfo)CurrentCulture.Clone();
                cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
            }

            var localDate = !String.IsNullOrWhiteSpace(dateString) ? DateTime.Parse(dateString, cultureInfo) : new DateTime(1980, 1, 1);
            var localTime = !String.IsNullOrWhiteSpace(timeString) ? DateTime.Parse(timeString, cultureInfo) : new DateTime(1980, 1, 1, 12, 0, 0);
            var localDateTime = new DateTime(localDate.Year, localDate.Month, localDate.Day, localTime.Hour, localTime.Minute, localTime.Second);

            if (!usingCultureCalendar) {
                var calendar = CurrentCalendar;
                localDateTime = calendar.ToDateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, localDateTime.Second, localDateTime.Millisecond);
            }

            return ConvertFromSiteTimeZone(localDateTime);
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