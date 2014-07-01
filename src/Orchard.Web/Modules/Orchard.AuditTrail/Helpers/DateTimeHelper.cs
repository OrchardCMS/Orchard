using System;

namespace Orchard.AuditTrail.Helpers {
    public static class DateTimeHelper {
        public static DateTime? Earliest(this DateTime? value) {
            if (value == null)
                return null;

            var v = value.Value;
            return new DateTime(v.Year, v.Month, v.Day, 0, 0, 0, 0, v.Kind);
        }

        public static DateTime? Latest(this DateTime? value) {
            if (value == null)
                return null;

            var v = value.Value;
            return new DateTime(v.Year, v.Month, v.Day, 23, 59, 59, 999, v.Kind);
        }
    }
}