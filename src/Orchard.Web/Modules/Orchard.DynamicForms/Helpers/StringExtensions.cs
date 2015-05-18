using System;

namespace Orchard.DynamicForms.Helpers {
    public static class StringExtensions {
        public static string WithDefault(this string value, string defaultValue) {
            return !String.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
    }
}