using System;

namespace Orchard.Layouts.Helpers {
    public static class StringHelper {
        
        public static int? ToInt32(this string value) {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            int i;
            if (Int32.TryParse(value, out i))
                return i;

            return null;
        }

        public static bool? ToBoolean(this string value) {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            bool b;
            if (Boolean.TryParse(value, out b))
                return b;

            return null;
        }

        public static string TrimSafe(this string value) {
            return value != null ? value.Trim() : null;
        }
    }
}