using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel {
        public bool InstallThemes { get; set; }
        public bool BrowseToGallery { get; set; }
        public ExtensionDescriptor CurrentTheme { get; set; }
        public IEnumerable<ThemeEntry> Themes { get; set; }
    }

    public class ThemeEntry {
        public ExtensionDescriptor Descriptor { get; set; }
        public bool Enabled { get; set; }
        public bool NeedsUpdate { get; set; }

        public string Id { get { return Descriptor.Id; } }
        public string Name { get { return Descriptor.Name; } }
        public string ThemePath(string path) {
            return Descriptor.Location + "/" + Descriptor.Id + path;
        }
    }
}