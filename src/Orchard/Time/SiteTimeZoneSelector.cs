using System.Web;
using Orchard.Settings;

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
            return new TimeZoneSelectorResult { Priority = 0, TimeZone = _workContextAccessor.GetContext().CurrentSite.TimeZone };
        }
    }
}
