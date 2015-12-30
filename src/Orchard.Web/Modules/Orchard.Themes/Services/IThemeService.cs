using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.Services {
    public interface IThemeService : IDependency {
        void DisableThemeFeatures(string themeName);
        void EnableThemeFeatures(string themeName);
        bool IsRecentlyInstalled(ExtensionDescriptor module);
        void DisablePreviewFeatures(IEnumerable<string> features);
    }
}