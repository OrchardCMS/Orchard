namespace Orchard.Templates.Helpers {
    public static class StringExtensions {
        public static string TrimSafe(this string value) {
            return value == null ? null : value.Trim();
        }
    }
}