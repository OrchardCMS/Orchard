using System.Collections.Generic;

namespace Orchard.Themes {
    public interface IThemeService : IDependency {
        ITheme GetCurrentTheme();
        ITheme GetThemeByName(string themeName);
        IEnumerable<ITheme> GetInstalledThemes();
        void SetCurrentTheme(string themeName);
    }
}
