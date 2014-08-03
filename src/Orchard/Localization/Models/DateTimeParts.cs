using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Orchard.Localization.Models {
    public struct DateTimeParts {

        //public static DateTimeParts? FromDateTime(DateTime? dateTime) {
        //    if (!dateTime.HasValue) {
        //        return null;
        //    }
        //    return FromDateTime(dateTime.Value);
        //}

        public static DateTimeParts FromDateTime(DateTime dateTime) {
            return new DateTimeParts(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
        }

        public DateTimeParts(int year, int month, int day, int hour, int minute, int second, int millisecond) {
            _date = new DateParts(year, month, day);
            _time = new TimeParts(hour, minute, second, millisecond);
        }

        public DateTimeParts(DateParts dateParts, TimeParts timeParts) {
            _date = dateParts;
            _time = timeParts;
        }

        private readonly DateParts _date;
        private readonly TimeParts _time;

        public DateParts Date {
            get {
                return _date;
            }
        }

        public TimeParts Time {
            get {
                return _time;
            }
        }

        public DateTime ToDateTime(Calendar calendar) {
            return calendar.ToDateTime(
                Date.Year > 0 ? Date.Year : DateTime.MinValue.Year,
                Date.Month > 0 ? Date.Month : DateTime.MinValue.Month,
                Date.Day > 0 ? Date.Day : DateTime.MinValue.Day,
                Time.Hour > 0 ? Time.Hour : DateTime.MinValue.Hour,
                Time.Minute > 0 ? Time.Minute : DateTime.MinValue.Minute,
                Time.Second > 0 ? Time.Second : DateTime.MinValue.Second,
                Time.Millisecond > 0 ? Time.Millisecond : DateTime.MinValue.Millisecond
            );
        }

        public override string ToString() {
            return String.Format("{0} {1}", _date, _time);
        }
    }
}
