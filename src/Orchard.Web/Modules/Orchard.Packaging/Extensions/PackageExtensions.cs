using System;
using System.IO;
using System.Linq;
using NuGet;
using Orchard.Environment.Extensions.Folders;
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

            if (packageId.Contains(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme))
                || packageId.Contains(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module))) {

                return isTheme
                    ? packageId.Substring(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Theme).Length)
                    : packageId.Substring(Services.PackagingSourceManager.GetExtensionPrefix(DefaultExtensionTypes.Module).Length);
            }
            else {
                return packageId;
            }
        }


        public static ExtensionDescriptor GetExtensionDescriptor(this IPackage package, string extensionType) {
            IPackageFile packageFile = package.GetFiles().FirstOrDefault(file => {
                var fileName = Path.GetFileName(file.Path);
                return fileName != null && fileName.Equals(
                    DefaultExtensionTypes.IsModule(extensionType) ? "module.txt" : "theme.txt",
                    StringComparison.OrdinalIgnoreCase);
            });

            if (packageFile != null) {
                var directoryName = Path.GetDirectoryName(packageFile.Path);
                if (directoryName != null) {
                    string extensionId = Path.GetFileName(directoryName.TrimEnd('/', '\\'));
                    using (var streamReader = new StreamReader(packageFile.GetStream())) {
                        return ExtensionHarvester.GetDescriptorForExtension("", extensionId, extensionType, streamReader.ReadToEnd());
                    }
                }
            }

            return null;
        }
    }
}