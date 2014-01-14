namespace Orchard.JobsQueue.Extensions {
    public static class StringExtensions {
        public static string TrimSafe(this string value) {
            return value != null ? value.Trim() : null;
        }
    }
}