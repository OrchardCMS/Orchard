using System.Collections.Generic;
using Orchard.Mvc.ViewModels;

namespace Orchard.Themes.ViewModels {
    public class ThemesIndexViewModel : BaseViewModel {
        public ITheme CurrentTheme { get; set; }
        public IEnumerable<ITheme> Themes { get; set; }
    }
}