using System.Web;
using Orchard.Settings;

namespace Orchard.Time {
    /// <summary>
    /// Implements <see cref="ITimeZoneSelector"/> by providing the timezone defined in the sites settings.
    /// </summary>
    public class SiteTimeZoneSelector : ITimeZoneSelector {
        private readonly ISiteService _siteService;

        public SiteTimeZoneSelector(ISiteService siteService) {
            _siteService = siteService;
        }

        public TimeZoneSelectorResult GetTimeZone(HttpContextBase context) {
            return new TimeZoneSelectorResult { Priority = 0, TimeZone = _siteService.GetSiteSettings().TimeZone};
        }
    }
}
