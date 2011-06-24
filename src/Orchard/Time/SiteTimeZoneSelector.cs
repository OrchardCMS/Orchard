using System;
using System.Web;

namespace Orchard.Time {
    /// <summary>
    /// Implements <see cref="ITimeZoneSelector"/> by providing the timezone defined in the sites settings.
    /// </summary>
    public class SiteTimeZoneSelector : ITimeZoneSelector {
        private readonly IWorkContextAccessor _workContextAccessor;

        public SiteTimeZoneSelector(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;
        }

        public TimeZoneSelectorResult GetTimeZone(HttpContextBase context) {
            var siteTimeZoneId = _workContextAccessor.GetContext(context).CurrentSite.SiteTimeZone;
            
            if (String.IsNullOrEmpty(siteTimeZoneId)) {
                return null;
            }

            return new TimeZoneSelectorResult {
                Priority = -5,
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(siteTimeZoneId)
            };
        }
    }
}
