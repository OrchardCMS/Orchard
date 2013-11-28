using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard;

namespace Orchard.Localization.Services {

    public class DefaultDateServices : IDateServices {

        private readonly IOrchardServices _orchardServices;
        private readonly IDateTimeLocalization _dateTimeLocalization;
        private readonly Lazy<CultureInfo> _cultureInfo;

        public DefaultDateServices(
            IOrchardServices orchardServices,
            IDateTimeLocalization dateTimeLocalization) {

            _orchardServices = orchardServices;
            _dateTimeLocalization = dateTimeLocalization;
            _cultureInfo = new Lazy<CultureInfo>(() => CultureInfo.GetCultureInfo(_orchardServices.WorkContext.CurrentCulture));
        }

        public virtual DateTime? ConvertToLocal(DateTime date) {
            return ConvertToLocal(ToNullable(date));
        }

        public virtual DateTime? ConvertToLocal(DateTime? date) {
            if (!date.HasValue) {
                return null;
            }
            return TimeZoneInfo.ConvertTimeFromUtc(date.Value, _orchardServices.WorkContext.CurrentTimeZone);
        }

        public virtual string ConvertToLocalString(DateTime date, string nullText = null) {
            return ConvertToLocalString(ToNullable(date), _dateTimeLocalization.LongDateTimeFormat.Text, nullText);
        }

        public virtual string ConvertToLocalString(DateTime date, string format, string nullText = null) {
            return ConvertToLocalString(ToNullable(date), format, nullText);
        }

        public virtual string ConvertToLocalString(DateTime? date, string format, string nullText = null) {
            var localDate = ConvertToLocal(date);
            if (!localDate.HasValue) {
                return nullText;
            }
            return localDate.Value.ToString(format, _cultureInfo.Value);
        }

        public virtual string ConvertToLocalDateString(DateTime date, string nullText = null) {
            return ConvertToLocalDateString(ToNullable(date), nullText);
        }

        public virtual string ConvertToLocalDateString(DateTime? date, string nullText = null) {
            return ConvertToLocalString(date, _dateTimeLocalization.ShortDateFormat.Text, nullText);
        }

        public virtual string ConvertToLocalTimeString(DateTime date, string nullText = null) {
            return ConvertToLocalTimeString(ToNullable(date), nullText);
        }

        public virtual string ConvertToLocalTimeString(DateTime? date, string nullText = null) {
            return ConvertToLocalString(date, _dateTimeLocalization.ShortTimeFormat.Text, nullText);
        }

        public virtual DateTime? ConvertFromLocal(DateTime date) {
            return ConvertToLocal(ToNullable(date));
        }

        public virtual DateTime? ConvertFromLocal(DateTime? date) {
            if (!date.HasValue) {
                return null;
            }
            return TimeZoneInfo.ConvertTimeToUtc(date.Value, _orchardServices.WorkContext.CurrentTimeZone);
        }

        public virtual DateTime? ConvertFromLocalString(string dateString) {
            if (String.IsNullOrWhiteSpace(dateString)) {
                return null;
            }
            var localDate = DateTime.Parse(dateString, _cultureInfo.Value);
            return ConvertFromLocal(localDate);
        }

        public virtual DateTime? ConvertFromLocalString(string dateString, string timeString) {
            if (String.IsNullOrWhiteSpace(dateString) && String.IsNullOrWhiteSpace(timeString)) {
                return null;
            }
            var localDate = !String.IsNullOrWhiteSpace(dateString) ? DateTime.Parse(dateString, _cultureInfo.Value) : new DateTime(1980, 1, 1);
            var localTime = !String.IsNullOrWhiteSpace(timeString) ? DateTime.Parse(timeString, _cultureInfo.Value) : new DateTime(1980, 1, 1, 12, 0, 0);
            var localDateTime = new DateTime(localDate.Year, localDate.Month, localDate.Day, localTime.Hour, localTime.Minute, localTime.Second);
            return ConvertFromLocal(localDateTime);
        }

        protected virtual DateTime? ToNullable(DateTime date) {
            return date == DateTime.MinValue ? new DateTime?() : new DateTime?(date);
        }
    }
}