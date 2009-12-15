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
        private readonly IThemeService _themeService;

        public SiteThemeSelector (IThemeService themeService) {
            _themeService = themeService;
        }

        
        public ThemeSelectorResult GetTheme(RequestContext context) {
            var theme = _themeService.GetSiteTheme();
            if (theme == null) {
                return null;
            }
            return new ThemeSelectorResult {Priority = -5, ThemeName = theme.ThemeName};
        }
    }
}
