using System;

namespace Orchard.Layouts.Helpers {
    public static class PrefixHelper {
        public static string AppendPrefix(this string currentPrefix, string additionalPrefix) {
            return String.IsNullOrWhiteSpace(currentPrefix) ? additionalPrefix : currentPrefix + "." + additionalPrefix;
        }
    }
}