using System.Collections.Generic;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel  {
        public ITheme CurrentTheme { get; set; }
        public IEnumerable<ITheme> Themes { get; set; }
    }
}