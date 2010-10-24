using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Orchard.Themes {
    public interface IThemeService : IDependency {
        ITheme GetThemeByName(string themeName);
        
        ITheme GetSiteTheme();
        void SetSiteTheme(string themeName);
        ITheme GetRequestTheme(RequestContext requestContext);

        void EnableTheme(string themeName);
        void DisableTheme(string themeName);

        IEnumerable<ITheme> GetInstalledThemes();
        IEnumerable<ITheme> GetEnabledThemes();
        void InstallTheme(HttpPostedFileBase file);
        void UninstallTheme(string themeName);
    }
}
