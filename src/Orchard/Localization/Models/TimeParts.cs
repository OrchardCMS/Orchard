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
            _hour = hour;
            _minute = minute;
            _second = second;
            _millisecond = millisecond;
            _kind = kind;
            _offset = offset;
        }

        private readonly int _hour;
        private readonly int _minute;
        private readonly int _second;
        private readonly int _millisecond;
        private readonly DateTimeKind _kind;
        private readonly TimeSpan _offset;

        public int Hour => _hour;

        public int Minute => _minute;

        public int Second => _second;

        public int Millisecond => _millisecond;

        public DateTimeKind Kind => _kind;

        public TimeSpan? Offset => _offset;

        public DateTime ToDateTime()
        {
            return new DateTime(
                DateTime.MinValue.Year,
                DateTime.MinValue.Month,
                DateTime.MinValue.Day,
                _hour > 0 ? _hour : DateTime.MinValue.Hour,
                _minute > 0 ? _minute : DateTime.MinValue.Minute,
                _second > 0 ? _second : DateTime.MinValue.Second,
                _millisecond > 0 ? _millisecond : DateTime.MinValue.Millisecond,
                _kind
            );
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}.{3}-{4}-{5}", _hour, _minute, _second, _millisecond, _kind, _offset);
        }
    }
}
