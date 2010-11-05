using System;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Themes.Models;

namespace Orchard.Themes.Services {
    [UsedImplicitly]
    public class SiteThemeSelector : IThemeSelector {
        private readonly IOrchardServices _orchardServices;

        public SiteThemeSelector(IOrchardServices orchardServices) {
            _orchardServices = orchardServices;
        }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            string currentThemeName = _orchardServices.WorkContext.CurrentSite.As<ThemeSiteSettingsPart>().Record.CurrentThemeName;

            if (String.IsNullOrEmpty(currentThemeName)) {
                return null;
            }

            return new ThemeSelectorResult { Priority = -5, ThemeName = currentThemeName };
        }
    }
}