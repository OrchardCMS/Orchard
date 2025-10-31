namespace Orchard.Layouts.Helpers
{
    public static class PrefixHelper
    {
        public static string AppendPrefix(this string currentPrefix, string additionalPrefix)
        {
            return string.IsNullOrWhiteSpace(currentPrefix) ? additionalPrefix : currentPrefix + "." + additionalPrefix;
        }
    }
}