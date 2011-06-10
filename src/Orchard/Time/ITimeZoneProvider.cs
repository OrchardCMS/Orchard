using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Orchard.Time {
    public interface ITimeZoneProvider : IDependency {
        TimeZoneInfo GetTimeZone(HttpContextBase context);
    }
}
