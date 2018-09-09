using System;
using System.Linq;

namespace Orchard.Glimpse.Extensions {
    public static class TimespanExtensions {
        public static string ToTimingString(this TimeSpan timespan) {
            return timespan.TotalMilliseconds.ToTimingString();
        }

        public static string ToTimingString(this double milliseconds) {
            return string.Format("{0} ms", milliseconds.ToString("0,0.00"));
        }

        public static string ToReadableString(this TimeSpan span) {
            var segments = new[] {
                GetTimeSpanSegment(span.Duration().Days, "day"),
                GetTimeSpanSegment(span.Duration().Hours, "hour"),
                GetTimeSpanSegment(span.Duration().Minutes, "minute"),
                GetTimeSpanSegment(span.Duration().Seconds, "second")
            };

            return string.Join(", ", segments.Where(s => s != null));
        }

        private static string GetTimeSpanSegment(int value, string unit) {
            return value > 0 ? string.Format("{0} {1}{2}", value.ToString("0"), unit, (value == 1 ? string.Empty : "s")) : null;
        }
    }
}