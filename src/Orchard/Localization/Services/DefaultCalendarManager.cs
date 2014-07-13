using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Orchard.Caching;
using Orchard.Data;
using Orchard.Localization.Records;

namespace Orchard.Localization.Services {
	public class DefaultCalendarManager : ICalendarManager {
		private readonly IEnumerable<ICalendarSelector> _calendarSelectors;

		public DefaultCalendarManager(IEnumerable<ICalendarSelector> calendarSelectors) {
			_calendarSelectors = calendarSelectors;
		}

		public IEnumerable<string> ListCalendars() {
			// Return all the calendar implementations in System.Globalization.
			// Could be done more dynamically using reflection, but that doesn't seem worth the performance hit.
			return new[] {
				"ChineseLunisolarCalendar",
				"GregorianCalendar",
				"HebrewCalendar",
				"HijriCalendar",
				"JapaneseCalendar",
				"JapaneseLunisolarCalendar",
				"JulianCalendar",
				"KoreanCalendar",
				"KoreanLunisolarCalendar",
				"PersianCalendar",
				"TaiwanCalendar",
				"TaiwanLunisolarCalendar",
				"ThaiBuddhistCalendar",
				"UmAlQuraCalendar"
			};
		}

		public string GetCurrentCalendar(HttpContextBase requestContext) {
			var requestCalendar = _calendarSelectors
				.Select(x => x.GetCalendar(requestContext))
				.Where(x => x != null)
				.OrderByDescending(x => x.Priority);

			if (!requestCalendar.Any())
				return String.Empty;

			foreach (var calendar in requestCalendar) {
				if (!String.IsNullOrEmpty(calendar.CalendarName)) {
					return calendar.CalendarName;
				}
			}

			return String.Empty;
		}

		public Calendar GetCalendarByName(string calendarName) {
			switch (calendarName) {
				case "ChineseLunisolarCalendar":
					return new ChineseLunisolarCalendar();
				case "GregorianCalendar":
					return new GregorianCalendar();
				case "HebrewCalendar":
					return new HebrewCalendar();
				case "HijriCalendar":
					return new HijriCalendar();
				case "JapaneseCalendar":
					return new JapaneseCalendar();
				case "JapaneseLunisolarCalendar":
					return new JapaneseLunisolarCalendar();
				case "JulianCalendar":
					return new JulianCalendar();
				case "KoreanCalendar":
					return new KoreanCalendar();
				case "KoreanLunisolarCalendar":
					return new KoreanLunisolarCalendar();
				case "PersianCalendar":
					return new PersianCalendar();
				case "TaiwanCalendar":
					return new TaiwanCalendar();
				case "TaiwanLunisolarCalendar":
					return new TaiwanLunisolarCalendar();
				case "ThaiBuddhistCalendar":
					return new ThaiBuddhistCalendar();
				case "UmAlQuraCalendar":
					return new UmAlQuraCalendar();
				default:
					throw new ArgumentException(String.Format("The calendar name '{0}' is not a recognized System.Globalization calendar name.", calendarName), "calendarName");
				
			}
		}
	}
}