using System;
using System.Globalization;

namespace Orchard.Localization.Models
{
    public struct DateParts
    {

        public static DateParts MinValue => new DateParts(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day);

        public DateParts(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public int Year { get; }
        public int Month { get; }
        public int Day { get; }

        public DateTime ToDateTime(Calendar calendar)
        {
            return new DateTime(
                Year > 0 ? Year : DateTime.MinValue.Year,
                Month > 0 ? Month : DateTime.MinValue.Month,
                Day > 0 ? Day : DateTime.MinValue.Day,
                DateTime.MinValue.Hour,
                DateTime.MinValue.Minute,
                DateTime.MinValue.Second,
                DateTime.MinValue.Millisecond,
                calendar
            );
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}-{2}", Year, Month, Day);
        }
    }
}
