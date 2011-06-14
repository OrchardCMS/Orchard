using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Time {
    public class CurrentTimeZoneWorkContext : IWorkContextStateProvider {
        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;

        public CurrentTimeZoneWorkContext(IEnumerable<ITimeZoneSelector> timeZoneSelectors) {
            _timeZoneSelectors = timeZoneSelectors;
        }

        public Func<WorkContext, T> Get<T>(string name) {
            if (name == "CurrentTimeZone") {
                return ctx => {
                    var timeZone = _timeZoneSelectors
                        .Select(x => x.GetTimeZone(ctx.HttpContext))
                        .Where(x => x != null)
                        .OrderByDescending(x => x.Priority)
                        .FirstOrDefault();

                    if (timeZone == null || timeZone.TimeZone == null) {
                        return (T)(object)TimeZoneInfo.Utc;
                    }

                    return (T)(object)timeZone.TimeZone;
                };
            }
            return null;
        }
    }
}
