using System;
using System.Globalization;
using Orchard.ContentManagement;
using Orchard.Settings;

namespace Orchard.Localization.Services {

	public class DefaultDateServices : IDateServices {

        private readonly IWorkContextAccessor _workContextAccessor;
		private readonly IDateTimeFormatProvider _dateTimeLocalization;
		private readonly ICalendarManager _calendarManager;

        public DefaultDateServices(
            IWorkContextAccessor workContextAccessor,
            IDateTimeFormatProvider dateTimeLocalization,
            ICalendarManager calendarManager) {
            _workContextAccessor = workContextAccessor;
            _dateTimeLocalization = dateTimeLocalization;
            _calendarManager = calendarManager;
        }

		public virtual DateTime? ConvertToLocal(DateTime date) {
			return ConvertToLocal(ToNullable(date));
		}

		public virtual DateTime? ConvertToLocal(DateTime? date) {
			if (!date.HasValue) {
				return null;
			}
            var workContext = _workContextAccessor.GetContext();
            return TimeZoneInfo.ConvertTimeFromUtc(date.Value, workContext.CurrentTimeZone);
		}

		public virtual string ConvertToLocalString(DateTime date, string nullText = null) {
			return ConvertToLocalString(ToNullable(date), _dateTimeLocalization.LongDateTimeFormat, nullText);
		}

		public virtual string ConvertToLocalString(DateTime date, string format, string nullText = null) {
			return ConvertToLocalString(ToNullable(date), format, nullText);
		}

		public virtual string ConvertToLocalString(DateTime? date, string format, string nullText = null) {
			var localDate = ConvertToLocal(date);
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

		public virtual DateTime? ConvertFromLocal(DateTime date) {
			return ConvertFromLocal(ToNullable(date));
		}

		public virtual DateTime? ConvertFromLocal(DateTime? date) {
			if (!date.HasValue) {
				return null;
			}
            var workContext = _workContextAccessor.GetContext();
			return TimeZoneInfo.ConvertTimeToUtc(date.Value, workContext.CurrentTimeZone);
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

			return ConvertFromLocal(localDate);
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

			return ConvertFromLocal(localDateTime);
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