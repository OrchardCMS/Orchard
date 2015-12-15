using System.Collections.Generic;
using Orchard.Themes.Models;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel {
        public bool InstallThemes { get; set; }
        public ThemeEntry CurrentTheme { get; set; }
        public IEnumerable<ThemeEntry> Themes { get; set; }
    }
}