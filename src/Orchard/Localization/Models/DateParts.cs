using System;
using System.Globalization;

namespace Orchard.Localization.Models
{
    public struct DateParts
    {

        public static DateParts MinValue => new DateParts(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day);

        public DateParts(int year, int month, int day)
        {
            _year = year;
            _month = month;
            _day = day;
        }

        private readonly int _day;
        private readonly int _month;
        private readonly int _year;

        public int Year => _year;
        public int Month => _month;
        public int Day => _day;

        public DateTime ToDateTime(Calendar calendar)
        {
            return new DateTime(
                _year > 0 ? _year : DateTime.MinValue.Year,
                _month > 0 ? _month : DateTime.MinValue.Month,
                _day > 0 ? _day : DateTime.MinValue.Day,
                DateTime.MinValue.Hour,
                DateTime.MinValue.Minute,
                DateTime.MinValue.Second,
                DateTime.MinValue.Millisecond,
                calendar
            );
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}", _year, _month, _day);
        }
    }
}
