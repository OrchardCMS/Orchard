using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard.Time {
    public class DefaultTimeZoneProvider : ITimeZoneProvider {
        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;

        public DefaultTimeZoneProvider(IEnumerable<ITimeZoneSelector> timeZoneSelectors) {
            _timeZoneSelectors = timeZoneSelectors;
        }

        public TimeZoneInfo GetTimeZone(HttpContextBase context) {
            var timeZone = _timeZoneSelectors
                .Select(x => x.GetTimeZone(context))
                .Where(x => x != null)
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault();

            if (timeZone == null || timeZone.TimeZone == null) {
                return TimeZoneInfo.Utc;
            }

            return timeZone.TimeZone;
        }
    }
}
