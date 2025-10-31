using System;

namespace Orchard.Localization.Models
{
    public struct TimeParts
    {

        public static TimeParts MinValue => new TimeParts(DateTime.MinValue.Hour, DateTime.MinValue.Minute, DateTime.MinValue.Second, DateTime.MinValue.Millisecond, DateTimeKind.Unspecified, offset: TimeSpan.Zero);

        public TimeParts(int hour, int minute, int second, int millisecond, DateTimeKind kind, TimeSpan offset)
        {
            if (kind == DateTimeKind.Utc && offset != TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(string.Format("The specified offset {0} does not match the specified kind {1}.", offset, kind));
            }
            Hour = hour;
            Minute = minute;
            Second = second;
            Millisecond = millisecond;
            Kind = kind;
            _offset = offset;
        }

        private readonly TimeSpan _offset;

        public int Hour { get; }

        public int Minute { get; }

        public int Second { get; }

        public int Millisecond { get; }

        public DateTimeKind Kind { get; }

        public TimeSpan? Offset => _offset;

        public DateTime ToDateTime()
        {
            return new DateTime(
                DateTime.MinValue.Year,
                DateTime.MinValue.Month,
                DateTime.MinValue.Day,
                Hour > 0 ? Hour : DateTime.MinValue.Hour,
                Minute > 0 ? Minute : DateTime.MinValue.Minute,
                Second > 0 ? Second : DateTime.MinValue.Second,
                Millisecond > 0 ? Millisecond : DateTime.MinValue.Millisecond,
                Kind
            );
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}.{3}-{4}-{5}", Hour, Minute, Second, Millisecond, Kind, _offset);
        }
    }
}
