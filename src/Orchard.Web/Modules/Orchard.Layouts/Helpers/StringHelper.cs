namespace Orchard.Layouts.Helpers {
    public static class StringHelper {
        
        public static string TrimSafe(this string value) {
            return value != null ? value.Trim() : null;
        }
    }
}