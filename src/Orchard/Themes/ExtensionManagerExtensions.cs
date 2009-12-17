using System.IO;
using System.Linq;
using Orchard.Extensions;

namespace Orchard.Themes {
    public static class ExtensionManagerExtensions {
        public static string GetThemeLocation(this IExtensionManager extensionManager, ITheme theme) {
            var themeDescriptor = extensionManager.AvailableExtensions()
                .Single(x => x.ExtensionType == "Theme" && x.Name == theme.ThemeName);

            return Path.Combine(themeDescriptor.Location, themeDescriptor.Name);
        }
    }
}