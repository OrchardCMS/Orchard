using System.IO;
using System.Linq;
using Orchard.Extensions;
using Orchard.Extensions.Models;

namespace Orchard.Themes {
    public static class ExtensionManagerExtensions {
        public static ExtensionDescriptor GetExtensionDescriptor(this IExtensionManager extensionManager, string extensionType, string extensionName) {
            return
                extensionManager.AvailableExtensions().FirstOrDefault(
                    ed => ed.ExtensionType == extensionType && ed.Name == extensionName);
        }

        public static string GetThemeLocation(this IExtensionManager extensionManager, ITheme theme) {
            ExtensionDescriptor descriptor = extensionManager.GetExtensionDescriptor("Theme", theme.ThemeName);
            
            return descriptor != null ? Path.Combine(descriptor.Location, descriptor.Name) : "~";
        }
    }
}