using System;
using System.Web;

namespace Orchard.Time {
    public interface ITimeZoneProvider : ISingletonDependency {
        TimeZoneInfo GetTimeZone(HttpContextBase context);
    }
}
