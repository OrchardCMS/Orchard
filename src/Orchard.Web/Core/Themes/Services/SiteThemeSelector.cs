using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Core.Themes.Models;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    public class SiteThemeSelector : IThemeSelector {

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            string currentThemeName = CurrentSite.As<ThemeSiteSettings>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
    }
}
