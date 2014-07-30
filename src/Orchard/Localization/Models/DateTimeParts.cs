using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Localization.Models {
    public struct DateTimeParts {

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

        public DateTime ToDateTime() {
            return new DateTime(
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
