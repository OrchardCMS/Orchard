using System;
using System.Web;
using Orchard.Logging;

namespace Orchard.Time {
    /// <summary>
    /// Implements <see cref="ITimeZoneSelector"/> by providing the timezone defined in the sites settings.
    /// </summary>
    public class SiteTimeZoneSelector : ITimeZoneSelector {
        private readonly IWorkContextAccessor _workContextAccessor;

        public SiteTimeZoneSelector(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public TimeZoneSelectorResult GetTimeZone(HttpContextBase context) {
            
            try {
                var siteTimeZoneId = _workContextAccessor.GetContext(context).CurrentSite.SiteTimeZone;

                if (String.IsNullOrEmpty(siteTimeZoneId)) {
                    return null;
                }

                return new TimeZoneSelectorResult {
                    Priority = -5,
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById(siteTimeZoneId)
                };
            }
            catch(Exception e) {
                Logger.Error(e, "TimeZone could not be loaded");

                // if the database could not be updated in time, ignore this provider
                return null;
            }
        }
    }
}
