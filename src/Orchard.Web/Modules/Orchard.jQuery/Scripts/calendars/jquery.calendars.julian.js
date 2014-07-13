/* http://keith-wood.name/calendars.html
   Julian calendar for jQuery v1.2.1.
   Written by Keith Wood (kbwood{at}iinet.com.au) August 2009.
   Available under the MIT (https://github.com/jquery/jquery/blob/master/MIT-LICENSE.txt) license. 
   Please attribute the author if you use it. */

(function($) { // Hide scope, no $ conflict

/* Implementation of the Julian calendar.
   Based on code from http://www.fourmilab.ch/documents/calendar/.
   See also http://en.wikipedia.org/wiki/Julian_calendar.
   @param  language  (string) the language code (default English) for localisation (optional) */
function JulianCalendar(language) {
	this.local = this.regional[language || ''] || this.regional[''];
}

JulianCalendar.prototype = new $.calendars.baseCalendar;

$.extend(JulianCalendar.prototype, {
	name: 'Julian', // The calendar name
	jdEpoch: 1721423.5, // Julian date of start of Persian epoch: 1 January 0001 AD = 30 December 0001 BCE
	daysPerMonth: [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31], // Days per month in a common year
	hasYearZero: false, // True if has a year zero, false if not
	minMonth: 1, // The minimum month number
	firstMonth: 1, // The first month in the year
	minDay: 1, // The minimum day number

	regional: { // Localisations
		'': {
			name: 'Julian', // The calendar name
			epochs: ['BC', 'AD'],
			monthNames: ['January', 'February', 'March', 'April', 'May', 'June',
			'July', 'August', 'September', 'October', 'November', 'December'],
			monthNamesShort: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
			dayNames: ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
			dayNamesShort: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
			dayNamesMin: ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'],
			dateFormat: 'mm/dd/yyyy', // See format options on parseDate
			firstDay: 0, // The first day of the week, Sun = 0, Mon = 1, ...
			isRTL: false // True if right-to-left language, false if left-to-right
		}
	},

	/* Determine whether this date is in a leap year.
	   @param  year  (CDate) the date to examine or
	                 (number) the year to examine
	   @return  (boolean) true if this is a leap year, false if not
	   @throws  error if an invalid year or a different calendar used */
	leapYear: function(year) {
		var date = this._validate(year, this.minMonth, this.minDay, $.calendars.local.invalidYear);
		var year = (date.year() < 0 ? date.year() + 1 : date.year()); // No year zero
		return (year % 4) == 0;
	},

	/* Determine the week of the year for a date - ISO 8601.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (number) the week of the year
	   @throws  error if an invalid date or a different calendar used */
	weekOfYear: function(year, month, day) {
		// Find Thursday of this week starting on Monday
		var checkDate = this.newDate(year, month, day);
		checkDate.add(4 - (checkDate.dayOfWeek() || 7), 'd');
		return Math.floor((checkDate.dayOfYear() - 1) / 7) + 1;
	},

	/* Retrieve the number of days in a month.
	   @param  year   (CDate) the date to examine or
	                  (number) the year of the month
	   @param  month  (number) the month
	   @return  (number) the number of days in this month
	   @throws  error if an invalid month/year or a different calendar used */
	daysInMonth: function(year, month) {
		var date = this._validate(year, month, this.minDay, $.calendars.local.invalidMonth);
		return this.daysPerMonth[date.month() - 1] +
			(date.month() == 2 && this.leapYear(date.year()) ? 1 : 0);
	},

	/* Determine whether this date is a week day.
	   @param  year   (CDate) the date to examine or
	                  (number) the year to examine
	   @param  month  (number) the month to examine
	   @param  day    (number) the day to examine
	   @return  (boolean) true if a week day, false if not
	   @throws  error if an invalid date or a different calendar used */
	weekDay: function(year, month, day) {
		return (this.dayOfWeek(year, month, day) || 7) < 6;
	},

	/* Retrieve the Julian date equivalent for this date,
	   i.e. days since January 1, 4713 BCE Greenwich noon.
	   @param  year   (CDate) the date to convert or
	                  (number) the year to convert
	   @param  month  (number) the month to convert
	   @param  day    (number) the day to convert
	   @return  (number) the equivalent Julian date
	   @throws  error if an invalid date or a different calendar used */
	toJD: function(year, month, day) {
		var date = this._validate(year, month, day, $.calendars.local.invalidDate);
		year = date.year();
		month = date.month();
		day = date.day();
		if (year < 0) { year++; } // No year zero
		// Jean Meeus algorithm, "Astronomical Algorithms", 1991
		if (month <= 2) {
			year--;
			month += 12;
		}
		return Math.floor(365.25 * (year + 4716)) +
			Math.floor(30.6001 * (month + 1)) + day - 1524.5;
	},

	/* Create a new date from a Julian date.
	   @param  jd  (number) the Julian date to convert
	   @return  (CDate) the equivalent date */
	fromJD: function(jd) {
		// Jean Meeus algorithm, "Astronomical Algorithms", 1991
		var a = Math.floor(jd + 0.5);
		var b = a + 1524;
		var c = Math.floor((b - 122.1) / 365.25);
		var d = Math.floor(365.25 * c);
		var e = Math.floor((b - d) / 30.6001);
		var month = e - Math.floor(e < 14 ? 1 : 13);
		var year = c - Math.floor(month > 2 ? 4716 : 4715);
		var day = b - d - Math.floor(30.6001 * e);
		if (year <= 0) { year--; } // No year zero
		return this.newDate(year, month, day);
	}
});

// Julian calendar implementation
$.calendars.calendars.julian = JulianCalendar;

})(jQuery);