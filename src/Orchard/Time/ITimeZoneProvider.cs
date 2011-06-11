using System;
using System.Web;

namespace Orchard.Time {
    public interface ITimeZoneProvider : IDependency {
        TimeZoneInfo GetTimeZone(HttpContextBase context);
    }
}
