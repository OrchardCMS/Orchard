using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace Orchard.Localization.Services {
    public interface ICalendarManager : IDependency {
        IEnumerable<string> ListCalendars();
		string GetCurrentCalendar(HttpContextBase requestContext);
		Calendar GetCalendarByName(string calendarName);
	}
}
