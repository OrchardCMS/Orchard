using System;

namespace Orchard.AuditTrail.Helpers {
    public static class StringExtensions {
        public static int? ToInt32(this string value) {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            int i;
            if(!Int32.TryParse(value, out i))
                return null;

            return i;
        }
    }
}