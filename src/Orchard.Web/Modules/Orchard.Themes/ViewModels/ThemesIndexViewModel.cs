using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel {
        public ExtensionDescriptor CurrentTheme { get; set; }
        public IEnumerable<ThemeEntry> Themes { get; set; }
    }

    public class ThemeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public bool Enabled { get; set; }
        public bool NeedsUpdate { get; set; }

        public string ThemeName { get { return Descriptor.Name; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public string ThemePath(string path) {
            return Descriptor.Location + "/" + Descriptor.Name + path;
        }
    }
}