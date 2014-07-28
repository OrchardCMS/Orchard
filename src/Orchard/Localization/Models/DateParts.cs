using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Localization.Models {
    public struct DateParts {

        public DateParts(int year, int month, int day) {
            _year = year;
            _month = month;
            _day = day;
        }

        private readonly int _day;
        private readonly int _month;
        private readonly int _year;

        public int Year {
            get {
                return _year;
            }
        }
        public int Month {
            get {
                return _month;
            }
        }
        public int Day {
            get {
                return _day;
            }
        }

        public override string ToString() {
            return String.Format("{0}-{1}-{2}", _year, _month, _day);
        }
    }
}
