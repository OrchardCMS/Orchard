using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.Themes.Models;

namespace Orchard.Themes.Services {
    [UsedImplicitly]
    public class SiteThemeSelector : IThemeSelector {

        protected virtual ISite CurrentSite { get; [UsedImplicitly] private set; }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            string currentThemeName = CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
    }
}