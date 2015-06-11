namespace Orchard.Azure.MediaServices.Helpers {
    public static class StringExtensions {
        public static string TrimSafe(this string value) {
            return value != null ? value.Trim() : null;
        }
    }
}