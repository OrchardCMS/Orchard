using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Themes {
    public interface IThemeService : IDependency {
        [Obsolete]
        ExtensionDescriptor GetThemeByName(string themeName);

        [Obsolete]
        ExtensionDescriptor GetRequestTheme(RequestContext requestContext);

        [Obsolete]
        void EnableTheme(string themeName);
        [Obsolete]
        void DisableTheme(string themeName);

        [Obsolete]
        IEnumerable<ExtensionDescriptor> GetInstalledThemes();
        [Obsolete]
        IEnumerable<ExtensionDescriptor> GetEnabledThemes();

        [Obsolete]
        void InstallTheme(HttpPostedFileBase file);
        [Obsolete]
        void UninstallTheme(string themeName);
    }
}
