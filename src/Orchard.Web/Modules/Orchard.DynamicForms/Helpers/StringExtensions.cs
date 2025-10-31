namespace Orchard.DynamicForms.Helpers
{
    public static class StringExtensions
    {
        public static string WithDefault(this string value, string defaultValue)
        {
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
    }
}