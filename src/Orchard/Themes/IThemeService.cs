using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes {
    public interface IThemeService : IDependency {
        [Obsolete]
        FeatureDescriptor GetThemeByName(string themeName);

        [Obsolete]
        FeatureDescriptor GetSiteTheme();
        [Obsolete]
        void SetSiteTheme(string themeName);

        [Obsolete]
        FeatureDescriptor GetRequestTheme(RequestContext requestContext);

        [Obsolete]
        void EnableTheme(string themeName);
        [Obsolete]
        void DisableTheme(string themeName);

        [Obsolete]
        IEnumerable<FeatureDescriptor> GetInstalledThemes();
        [Obsolete]
        IEnumerable<FeatureDescriptor> GetEnabledThemes();

        [Obsolete]
        void InstallTheme(HttpPostedFileBase file);
        [Obsolete]
        void UninstallTheme(string themeName);
    }
}
