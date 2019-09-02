using System;
using System.Web;

namespace Orchard.Time {
    /// <summary>
    /// Implements <see cref="ITimeZoneSelector"/> by providing the timezone defined in the machine's local settings.
    /// </summary>
    public class FallbackTimeZoneSelector : ITimeZoneSelector {
        public TimeZoneSelectorResult GetTimeZone(HttpContextBase context) {
            return new TimeZoneSelectorResult {
                Priority = -100,
                TimeZone = TimeZoneInfo.Local
            };
        }
    }
}