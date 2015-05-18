using System;

namespace Orchard.AuditTrail.Helpers {
    public static class DateTimeExtensions {
        public static DateTime StartOfDay(this DateTime value) {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0, value.Kind);
        }

        public static DateTime? StartOfDay(this DateTime? value) {
            if (value == null)
                return null;

            return StartOfDay(value.Value);
        }

        public static DateTime EndOfDay(this DateTime value) {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59, 999, value.Kind);
        }

        public static DateTime? EndOfDay(this DateTime? value) {
            if (value == null)
                return null;

            var v = value.Value;
            return new DateTime(v.Year, v.Month, v.Day, 23, 59, 59, 999, v.Kind);
        }
    }
}