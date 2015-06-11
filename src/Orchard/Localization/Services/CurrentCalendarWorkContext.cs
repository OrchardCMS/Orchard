using System;

namespace Orchard.Localization.Services {
    public class CurrentCalendarWorkContext : IWorkContextStateProvider {
        private readonly ICalendarManager _calendarManager;

		public CurrentCalendarWorkContext(ICalendarManager calendarManager) {
			_calendarManager = calendarManager;
        }

        public Func<WorkContext, T> Get<T>(string name) {
            if (name == "CurrentCalendar") {
				return ctx => (T)(object)_calendarManager.GetCurrentCalendar(ctx.HttpContext);
            }
            return null;
        }
    }
}
