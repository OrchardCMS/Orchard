using System.Collections.Generic;
using System.Web;
using System.Web.Routing;

namespace Orchard.Themes {
    public interface IThemeService : IDependency {
        ITheme GetThemeByName(string themeName);
        
        ITheme GetSiteTheme();
        void SetSiteTheme(string themeName);
        ITheme GetRequestTheme(RequestContext requestContext);

        IEnumerable<ITheme> GetInstalledThemes();
        void InstallTheme(HttpPostedFileBase file);
        void UninstallTheme(string themeName);
    }
}
