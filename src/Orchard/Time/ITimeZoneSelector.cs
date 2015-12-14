using System.Web;

namespace Orchard.Time {
    public interface ITimeZoneSelector : IDependency {
        TimeZoneSelectorResult GetTimeZone(HttpContextBase context);
    }
}
