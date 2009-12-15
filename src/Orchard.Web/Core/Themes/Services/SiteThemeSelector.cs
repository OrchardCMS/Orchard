using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.Core.Themes.Models;
using Orchard.Models;
using Orchard.Settings;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    public class SiteThemeSelector : IThemeSelector {

        public ISite CurrentSite { get; set; }
        
        public ThemeSelectorResult GetTheme(RequestContext context) {
            string currentThemeName = CurrentSite.As<ThemeSiteSettings>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
    }
}
