using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Framework.Localization.Models {
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

        public override string ToString() {
            return String.Format("{0} {1}", _date, _time);
        }
    }
}
