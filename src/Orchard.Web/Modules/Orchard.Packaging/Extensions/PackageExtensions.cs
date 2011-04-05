using NuGet;
using Orchard.Environment.Extensions.Models;
using Orchard.Packaging.Models;

namespace Orchard.Packaging.Extensions {
    public static class PackageExtensions {
        public static bool IsTheme(this IPackage package) {
            return IsTheme(package.Id);
        }

        public static bool IsTheme(this PackagingEntry packagingEntry) {
            return IsTheme(packagingEntry.PackageId);
        }

        public static string ExtensionFolder(this IPackage package) {
            return ExtensionFolder(package.IsTheme());
        }

        public static string ExtensionFolder(this PackagingEntry packagingEntry) {
            return ExtensionFolder(packagingEntry.IsTheme());
        }

        public static string ExtensionId(this IPackage package) {
            return ExtensionId(package.IsTheme(), package.Id);
        }

        public static string ExtensionId(this PackagingEntry packagingEntry) {
            return ExtensionId(packagingEntry.IsTheme(), packagingEntry.PackageId);
        }

        private static bool IsTheme(string packageId) {
            return packageId.StartsWith(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme));
        }

        private static string ExtensionFolder(bool isTheme) {
            return isTheme ? "Themes" : "Modules";
        }

        private static string ExtensionId(bool isTheme, string packageId) {
            return isTheme ?
                packageId.Substring(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme).Length) :
                packageId.Substring(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module).Length);
        }
    }
}