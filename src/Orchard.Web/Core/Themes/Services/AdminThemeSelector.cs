using System.Globalization;
using System.IO;
using System.Web.Routing;
using Orchard.Settings;
using Orchard.Themes;

namespace Orchard.Core.Themes.Services {
    public class AdminThemeSelector : IThemeSelector {
        private readonly ISiteService _siteService;

        public AdminThemeSelector(ISiteService siteService) {
            _siteService = siteService;
        }

        public ThemeSelectorResult GetTheme(RequestContext context) {
            var siteUrl = _siteService.GetSiteSettings().SiteUrl;
            //todo: (heskew) get at the admin path in a less hacky way
            if (!context.HttpContext.Request.Path.StartsWith(Path.Combine(siteUrl, "admin").Replace("\\", "/"), true, CultureInfo.InvariantCulture))
                return null;

            return new ThemeSelectorResult { Priority = 100, ThemeName = "TheAdmin" };
        }
    }
}
