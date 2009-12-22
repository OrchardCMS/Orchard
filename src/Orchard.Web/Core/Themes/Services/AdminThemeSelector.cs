using System.Globalization;
using System.IO;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.Core.Settings.Models;
using Orchard.Settings;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    public class AdminThemeSelector : IThemeSelector {
        private readonly ISiteService _siteService;

        public AdminThemeSelector(ISiteService siteService) {
            _siteService = siteService;
        }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            //todo: (heskew) get at the SiteUrl the "right" way. or is this the right way :|
            var siteUrl = _siteService.GetSiteSettings().ContentItem.As<SiteSettings>().Record.SiteUrl;

            if (!context.HttpContext.Request.Path.StartsWith(Path.Combine(siteUrl, "admin"), true, CultureInfo.InvariantCulture))
                return null;

            return new ThemeSelectorResult { Priority = 0, ThemeName = "TheAdmin" };
        }
    }
}
