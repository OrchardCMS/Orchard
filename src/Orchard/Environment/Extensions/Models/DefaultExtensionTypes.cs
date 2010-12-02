using System;

namespace Orchard.Environment.Extensions.Models {
    public static class DefaultExtensionTypes {
        public const string Module = "Module";
        public const string Theme = "Theme";
        public const string Core = "Core";

        public static bool IsModule(string extensionType) {
            return string.Equals(extensionType, Module, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTheme(string extensionType) {
            return string.Equals(extensionType, Theme, StringComparison.OrdinalIgnoreCase);
        }
    }
}